using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(FunctionsV2Sample.Startup))]
namespace FunctionsV2Sample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddCosmosContextForFunctionsV2();
            builder.Services.AddSingleton<ICarRepository, CarRepository>();
        }
    }
}
