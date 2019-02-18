using AzureCosmosContext.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AzureCosmosContext.Extensions
{
    public static class CosmosContextExtensions
    {
        public static IServiceCollection AddCosmosContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<CosmosOptions>(configuration.GetSection("cosmosOptions"));

            services.AddSingleton(sp =>
            {
                var cosmosOptions = sp.GetService<IOptions<CosmosOptions>>().Value;
                cosmosOptions.Validate();

                var connectionString = configuration.GetSection("cosmosDbConnectionString").Get<string>();

                var config = new CosmosConfiguration(connectionString);

                // ConnectionPolicy のDefault値は、SDKのConnectionPolicy.csで設定されています。

                // ## Connection Mode について
                // Default で ConnectionMode.Direct/Protocol.Tcp で接続されます。
                // もしGateway(ConnectionMode.Gateway/Protocol.Https) を使いたければ、以下メソッドを呼ぶ
                // config.UseConnectionModeGateway(maxConnectionLimit: ??);

                // CamelCase Serialize/Deserialize support
                config.UseCustomJsonSerializer(new CosmosCamelCaseJsonSerializer());

                if (cosmosOptions.ThrottlingRetryOptions != null)
                {
                    config.UseThrottlingRetryOptions(cosmosOptions.ThrottlingRetryOptions.MaxRetryWaitTimeOnThrottledRequests,
                                                     cosmosOptions.ThrottlingRetryOptions.MaxRetryAttemptsOnThrottledRequests);
                }

                // multi-master support
                if (!string.IsNullOrEmpty(cosmosOptions.CurrentRegion))
                {
                    config.UseCurrentRegion(cosmosOptions.CurrentRegion);
                }

                return new CosmosClient(config);
            });

            services.AddSingleton<CosmosContext>();

            return services;
        }
    }
}