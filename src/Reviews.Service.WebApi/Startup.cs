using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
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
            
            /*
            eventStore cluster connection builder
            var eventStoreConnection= await EventStoreConnectionBuilder.ConfigureStore();
            */
            var aggregateStore = new GesAggregateStore(eventStoreConnection,null);
            
            services.AddSingleton(new ApplicationService(aggregateStore));
            
            var documentStore = RavenDbConfiguration.Build(Configuration["RavenDb:Url"], Configuration["RavenDb:Database"]);
            
            IDocumentSession GetSession() => documentStore.OpenSession();
            
            await ProjectionManager.With
                .Connection(eventStoreConnection)
                .CheckpointStore(new RavenDbCheckPointStore(GetSession))
                .SetProjections( new Projection[]
                {
                    new ActiveReviews(GetSession),
                    new ReviewsByOwner(GetSession),
                    new ReviewsByProducts(GetSession),   
                })
                .StartAll();
        }
    }
}
