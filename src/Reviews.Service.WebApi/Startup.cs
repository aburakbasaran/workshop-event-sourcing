using System;
using System.Reflection;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Reviews.Core.EventStore;
using Reviews.Core;
using Reviews.Core.Projections;
using Reviews.Core.Projections.RavenDb;
using Reviews.Core.Snapshots.Providers;
using Reviews.Core.Snapshots.Providers.InMemory;
using Reviews.Service.WebApi.Modules.Reviews;
using Reviews.Service.WebApi.Modules.Reviews.Projections;
using Swashbuckle.AspNetCore.Swagger;
using ICheckpointStore = Reviews.Core.Projections.ICheckpointStore;


namespace Reviews.Service.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureServicesAsync(services).GetAwaiter().GetResult();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvcWithDefaultRoute();
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint(
                Configuration["Swagger:Endpoint:Url"], 
                Configuration["Swagger:Endpoint:Name"]));
            
            app.UseMvc();
        }
        
        private async Task ConfigureServicesAsync(IServiceCollection services)
        {
            //Building Event store components
            BuildEventStore(services);
            
            EventMappings.MapEventTypes();
            
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    $"v{Configuration["Swagger:Version"]}", 
                    new Info {
                        Title   = Configuration["Swagger:Title"], 
                        Version = $"v{Configuration["Swagger:Version"]}"
                    });
            });
        }

        private void RegisterDependecies(IServiceCollection service)
        {
        }

        private async Task BuildEventStore(IServiceCollection services)
        {
            //Create EventStore Connection
            var eventStoreConnection = EventStoreConnection.Create(
                Configuration["EventStore:ConnectionString"],
                ConnectionSettings.Create()
                    .KeepReconnecting()
                    .EnableVerboseLogging()
                    .SetHeartbeatInterval(TimeSpan.FromMilliseconds(5 * 1000))
                    .UseDebugLogger(),
                Environment.ApplicationName
            );
            
            eventStoreConnection.Connected += (sender, args) 
                => Console.WriteLine($"Connection to {args.RemoteEndPoint} event store established.");
            
            eventStoreConnection.ErrorOccurred += (sender, args) 
                => Console.WriteLine($"Connection error : {args.Exception}");
            
            await eventStoreConnection.ConnectAsync();
            
            var aggregateStore = new GesAggrigateStore(
                eventStoreConnection, 
                (type, id) => $"{type.Name}-{id}", 
                null);

            var gesSnapshotStore = new GesSnapshotStore(eventStoreConnection,
                (type, id) => $"{type.Name}-{id}",
                null);

            
            var inMemorySnapshotStore = new InMemorySnapshotStorageProvider("memoryDumpFile.dats");
            
            var redisSnapshotStore = new RedisSnapshotStoreProvider(Configuration["RedisStore:ConnectionString"]);
            
            var repository = new Repository(aggregateStore,gesSnapshotStore);

            services.AddSingleton<IRepository>(repository);
            
            services.AddSingleton(new ApplicationService(repository));

            IAsyncDocumentSession GetSession() => BuildRevenDb().OpenAsyncSession();
            
            await ProjectionManager.With
                .Connection(eventStoreConnection)
                .CheckpointStore(new RavenDbChecklpointStore(GetSession))
                .SetProjections( new Projection[]
                {
                    new ActiveReviews(GetSession),
                    new ReviewsByOwner(GetSession)  
                })
                .StartAll();
        }

        private IDocumentStore BuildRevenDb()
        {
            var store = new DocumentStore {
                Urls     = new[] {Configuration["RavenDb:Url"]},
                Database = Configuration["RavenDb:Database"]
            };
            
            if (Environment.IsDevelopment()) store.OnBeforeQuery += (_, args) 
                => args.QueryCustomization.WaitForNonStaleResults();

            try 
            {
                store.Initialize();                
                Console.WriteLine($"Connection to {store.Urls[0]} document store established.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"Failed to establish connection to \"{store.Urls[0]}\" document store!" +
                    $"Please check if https is properly configured in order to use the certificate.", ex);
            }
            try
            {
                var record = store.Maintenance.Server.Send(new GetDatabaseRecordOperation(store.Database));
                if (record == null) 
                {
                    store.Maintenance.Server
                        .Send(new CreateDatabaseOperation(new DatabaseRecord(store.Database)));

                    Console.WriteLine($"{store.Database} document store database created.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"Failed to ensure that \"{store.Database}\" document store database exists!", ex);
            }
            
            try
            {
                IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store);
                Console.WriteLine($"{store.Database} document store database indexes created or updated.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to create or update \"{store.Database}\" document store database indexes!", ex);
            }
            
            return store;
            
        }
    }
}
