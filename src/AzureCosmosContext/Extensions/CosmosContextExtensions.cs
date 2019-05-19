﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static IServiceCollection AddCosmosContextForFunctionsV2(this IServiceCollection services)
        {
            var defaultConfig = services.First(s => s.ServiceType == typeof(IConfiguration)).ImplementationInstance as IConfiguration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var customConfig = builder.AddConfiguration(defaultConfig).Build();

            return AddCosmosContext(services, customConfig);
        }









        public static IServiceCollection AddCosmosContext(this IServiceCollection services)
        {
            var configuration = services.First(s => s.ServiceType == typeof(IConfiguration)).ImplementationInstance as IConfiguration;
            return AddCosmosContext(services, configuration);
        }

        public static IServiceCollection AddCosmosContext(this IServiceCollection services, IConfiguration configuration)
        {
            GuardNotNull(services, nameof(services));
            GuardNotNull(configuration, nameof(configuration));

            services.AddOptions();
            services.Configure<CosmosOptions>(configuration.GetSection("cosmosOptions"));

            services.AddSingleton(sp =>
            {
                var cosmosOptions = sp.GetService<IOptions<CosmosOptions>>().Value;
                var connectionString = configuration.GetSection("cosmosDbConnectionString").Get<string>();

                return CreateCosmosClient(connectionString, cosmosOptions);
            });

            services.AddSingleton<CosmosContext>();

            return services;
        }

        public static IServiceCollection AddCosmosContext(this IServiceCollection services, string connectionString, CosmosOptions cosmosOptions)
        {
            GuardNotNull(services, nameof(services));

            services.AddSingleton(_ = CreateCosmosClient(connectionString, cosmosOptions));
            services.AddSingleton<CosmosContext>();

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <remarks>初回起動時の遅延対策として、アプリ起動時にインスタンス化してCosmos DBと接続します。</remarks>
        public static IApplicationBuilder UseCosmosContext(this IApplicationBuilder app)
        {
            GuardNotNull(app, nameof(app));

            app.ApplicationServices.GetService<CosmosContext>();
            return app;
        }

        #region private

        private static CosmosClient CreateCosmosClient(string connectionString, CosmosOptions cosmosOptions)
        {
            GuardEmptyString(connectionString, "cosmosDbConnectionString");
            cosmosOptions.Guard();

            var builder = new CosmosClientBuilder(connectionString);

            // ConnectionPolicy のDefault値は、SDKのConnectionPolicy.csで設定されています。

            // ## Connection Mode について
            // Default で ConnectionMode.Direct/Protocol.Tcp で接続されます。
            // もしGateway(ConnectionMode.Gateway/Protocol.Https) を使いたければ、以下メソッドを呼ぶ
            // builder.UseConnectionModeGateway(maxConnectionLimit: ??);

            // Default: CamelCase Serialize/Deserialize and ignore Readonly property
            // TODO: 設定変更用のconfigは未実装
            //var settings = JsonSerializerSettingsFactory.CreateForReadonlyIgnoreAndCamelCase();
            var settings = JsonSerializerSettingsFactory.CreateForCamelCase();

            builder.UseCustomJsonSerializer(new CustomizableCaseJsonSerializer(settings));


            if (cosmosOptions.ThrottlingRetryOptions != null)
            {

                builder.UseThrottlingRetryOptions(cosmosOptions.ThrottlingRetryOptions.MaxRetryWaitTimeOnThrottledRequests,
                    cosmosOptions.ThrottlingRetryOptions.MaxRetryAttemptsOnThrottledRequests);
            }

            // multi-master support
            if (!string.IsNullOrEmpty(cosmosOptions.CurrentRegion))
            {
                builder.UseCurrentRegion(cosmosOptions.CurrentRegion);
            }

            return builder.Build();
        }

        [DebuggerStepThrough]
        private static void GuardNotNull<T>(T value, string propertyName) where T : class
        {
            if (value == null) throw new ArgumentNullException(propertyName);
        }

        [DebuggerStepThrough]
        private static void GuardEmptyString(string target, string propertyName)
        {
            if (string.IsNullOrEmpty(target)) throw new ArgumentException(propertyName);
        }

        #endregion
    }
}