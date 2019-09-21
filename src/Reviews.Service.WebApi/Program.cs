using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using static System.Environment;

namespace Reviews.Service.WebApi
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            
           await Console.Error.WriteLineAsync($"Application starting ({GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"})...");

            try
            {
                var configuration = BuildConfiguration(args);
                await Console.Error.WriteLineAsync("Configuration built successfully.");
                await CreateWebHostBuilder(args)
                    .UseConfiguration(configuration)
                    .ConfigureServices(services => services.AddSingleton(configuration))
                    .Build().RunAsync();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        
        private static IConfiguration BuildConfiguration(string[] args)
            => new ConfigurationBuilder()
                .SetBasePath(CurrentDirectory)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
    }
    
    
}
