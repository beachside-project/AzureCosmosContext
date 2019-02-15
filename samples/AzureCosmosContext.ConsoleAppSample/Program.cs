using System.Reflection;
using AzureCosmosContext.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureCosmosContext.ConsoleAppSample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables("NETCORE_");
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {

                    var env = hostContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile("appsettings." + hostContext.HostingEnvironment.EnvironmentName + ".json", true, true);

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        // use userSecrets if need. 
                    }
                    else
                    {
                        config.AddEnvironmentVariables();
                    }
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddCosmosContext(hostContext.Configuration);

                    services.AddSingleton<ICarRepository, CarRepository>();
                    services.AddSingleton<IHostedService, CosmosSample>();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                });

            await builder.RunConsoleAsync();
        }
    }
}