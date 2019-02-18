using System;
using System.Diagnostics;
using AzureCosmosContext;
using AzureCosmosContext.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosContextExtensions
    {
        public static IServiceCollection AddCosmosContext(this IServiceCollection services, IConfiguration configuration)
        {
            ValidateNotNull(services, nameof(services));
            ValidateNotNull(configuration, nameof(configuration));

            services.AddOptions();
            services.Configure<CosmosOptions>(configuration.GetSection("cosmosOptions"));

            services.AddSingleton(sp =>
            {
                var cosmosOptions = sp.GetService<IOptions<CosmosOptions>>().Value;
                cosmosOptions.Validate();

                var connectionString = configuration.GetSection("cosmosDbConnectionString").Get<string>();
                ValidateNotNull(connectionString, "cosmosDbConnectionString");
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

        public static IApplicationBuilder UseCosmosContext(this IApplicationBuilder app)
        {
            ValidateNotNull(app, nameof(app));

            // 初回起動時の遅延対策として、アプリ起動時にインスタンス化してCosmos DBと接続
            app.ApplicationServices.GetService<CosmosContext>();
            return app;
        }

        [DebuggerStepThrough]
        private static void ValidateNotNull<T>(T value, string name) where T : class
        {
            if (value == null) throw new ArgumentNullException(name);
        }
    }
}