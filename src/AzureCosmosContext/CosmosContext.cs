using AzureCosmosContext.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureCosmosContext.Extensions;

namespace AzureCosmosContext
{
    // 現時点では、Database は一つしか使わない前提。
    public class CosmosContext
    {
        private const string MessageOfDatabaseNotExists = "Database instance does not found!(DatabaseId: {DatabaseId})";
        private const string MessageOfContainerNotExists = "Container definition ERROR! Please check Containers of appsettings.json and Cosmos DB instance!(DtabaseId: {DatabaseId})";

        public Database Database { get; private set; }
        public Dictionary<string, Container> Containers { get; } = new Dictionary<string, Container>();

        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<CosmosContext> _logger;
        private readonly CosmosOptions _cosmosOptions;

        public CosmosContext(CosmosClient cosmosClient, IOptions<CosmosOptions> options, ILogger<CosmosContext> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
            _cosmosOptions = options.Value;
            _cosmosOptions.Guard();
            SetupAsync().GetAwaiter().GetResult();
        }

        private async Task SetupAsync()
        {
            if (_cosmosOptions.NeedCreateIfExist)
            {
                await InitializeCosmosDbAsync();
            }
            else
            {
                await CacheDatabaseAsync();
            }
            await CacheContainersAsync();
        }

        private async Task InitializeCosmosDbAsync()
        {
            // appsettings の DefaultThroughput は Database の Throughput へ設定  
            Database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_cosmosOptions.DatabaseId, _cosmosOptions.DefaultThroughput);

            var throughputResponse = await Database.ReadThroughputAsync();
            _logger.LogInformation($"Database:{Database.Id}; throughput:{ConvertThroughputLogString(throughputResponse.Resource.Throughput)}");

            await _cosmosOptions.CosmosContainerOptions
                .Select(async op => await CreateContainerIfNotExistsAsync(op))
                .WhenAll();
        }

        private async Task CreateContainerIfNotExistsAsync(CosmosContainerOptions options)
        {
            // TODO: Container単位のDefaultThroughputの実装
            Container container = await Database.CreateContainerIfNotExistsAsync(options.ToContainerProperties());
            var throughputResponse = await container.ReadThroughputAsync();

            _logger.LogInformation($"Container:{container.Id}; throughput:{ConvertThroughputLogString(throughputResponse?.Resource?.Throughput)}");
        }

        private async Task CacheDatabaseAsync()
        {
            var databaseResponse = await _cosmosClient.GetDatabase(_cosmosOptions.DatabaseId).ReadAsync();

            if (databaseResponse.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogCritical(MessageOfDatabaseNotExists, _cosmosOptions.DatabaseId);
                throw new ArgumentException(MessageOfDatabaseNotExists, _cosmosOptions.DatabaseId);
            }

            Database = databaseResponse;
        }

        private async Task CacheContainersAsync()
        {
            var iterator = Database.GetContainerIterator();

            while (iterator.HasMoreResults)
            {
                foreach (var containerProperties in await iterator.ReadNextAsync())
                {
                    Containers.Add(containerProperties.Id, Database.GetContainer(containerProperties.Id));
                }
            }

            ValidateContainers();
        }

        private void ValidateContainers(bool needStrongCheck = false)
        {
            if (Containers.Count == 0)
            {
                _logger.LogCritical(MessageOfContainerNotExists, _cosmosOptions.DatabaseId);
                throw new ArgumentException(MessageOfContainerNotExists, _cosmosOptions.DatabaseId);
            }

            if (needStrongCheck && !IsValidContainersSetting())
            {
                _logger.LogCritical(MessageOfContainerNotExists, _cosmosOptions.DatabaseId);
                throw new ArgumentException(MessageOfContainerNotExists, _cosmosOptions.DatabaseId);
            }
        }

        private static string ConvertThroughputLogString(int? throughput) => throughput == null ? "No Setting" : throughput.ToString();

        private bool IsValidContainersSetting() => _cosmosOptions.CosmosContainerOptions.All(options => Containers.ContainsKey(options.ContainerId));
    }
}