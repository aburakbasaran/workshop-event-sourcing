using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents.Session;
using Reviews.Core.EventStore;
using Reviews.Core;
using Reviews.Core.Projections;
using Reviews.Core.Projections.RavenDb;
using Reviews.Service.WebApi.Modules.Reviews;
using Reviews.Service.WebApi.Modules.Reviews.Projections;
using Swashbuckle.AspNetCore.Swagger;


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
            
            var repository = new Repository(aggregateStore,gesSnapshotStore);

            services.AddSingleton<IRepository>(repository);
            
            services.AddSingleton(new ApplicationService(repository));
            var documentStore = RavenDbConfiguration.Build(Configuration["RavenDb:Url"], Configuration["RavenDb:Database"]);
            
            IAsyncDocumentSession GetSession() => documentStore.OpenAsyncSession();
            
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
    }
}
