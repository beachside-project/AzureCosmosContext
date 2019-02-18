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

        public CosmosDatabase Database { get; private set; }
        public CosmosContainers Containers { get; private set; }
        public IList<string> ContainerIds { get; private set; }

        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<CosmosContext> _logger;
        private readonly CosmosOptions _cosmosDbOptions;


        public CosmosContext(CosmosClient cosmosClient, IOptions<CosmosOptions> options, ILogger<CosmosContext> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
            _cosmosDbOptions = options.Value;
            SetupAsync().Wait();
        }

        private async Task SetupAsync()
        {
            if (_cosmosDbOptions.NeedCreateIfExist)
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
            var databaseResponse = await _cosmosClient.Databases.CreateDatabaseIfNotExistsAsync(_cosmosDbOptions.DatabaseId, _cosmosDbOptions.DefaultThroughput);
            Database = databaseResponse;

            var dbThroughput = await Database.ReadProvisionedThroughputAsync();
            _logger.LogInformation($"Database:{Database.Id}; throughput:{ConvertThroughputLogString(dbThroughput)}");

            await _cosmosDbOptions.CosmosContainerOptions
                .Select(async op => await CreateContainerIfNotExistsAsync(op))
                .WhenAll();

            Containers = Database.Containers;
        }

        private async Task CreateContainerIfNotExistsAsync(CosmosContainerOptions options)
        {
            // TODO: Container単位のDefaultThroughputの実装
            CosmosContainer container = await Database.Containers.CreateContainerIfNotExistsAsync(options.ToCosmosContainerSettings());
            var throughput = await container.ReadProvisionedThroughputAsync();

            _logger.LogInformation($"Container:{container}; throughput:{ConvertThroughputLogString(throughput)}");
        }

        private async Task CacheDatabaseAsync()
        {
            var databaseResponse = await _cosmosClient.Databases[_cosmosDbOptions.DatabaseId].ReadAsync();
            if (databaseResponse.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogCritical(MessageOfDatabaseNotExists, _cosmosDbOptions.DatabaseId);
                throw new ArgumentException(MessageOfDatabaseNotExists, _cosmosDbOptions.DatabaseId);
            }

            Database = _cosmosClient.Databases[_cosmosDbOptions.DatabaseId];
        }

        private async Task CacheContainersAsync()
        {
            var resultSetIterator = Database.Containers.GetContainerIterator();
            ContainerIds = new List<string>();
            while (resultSetIterator.HasMoreResults)
            {
                foreach (var container in await resultSetIterator.FetchNextSetAsync())
                {
                    ContainerIds.Add(container.Id);
                }
            }

            // ここまでチェックする必要があるかは用途次第。
            if (ContainerIds.Count == 0 || !IsValidContainersSetting())
            {
                _logger.LogCritical(MessageOfContainerNotExists, _cosmosDbOptions.DatabaseId);
                throw new ArgumentException(MessageOfContainerNotExists, _cosmosDbOptions.DatabaseId);
            }

            Containers = Database.Containers;
        }

        private static string ConvertThroughputLogString(int? throughput) => throughput == null ? "No Setting" : throughput.ToString();

        private bool IsValidContainersSetting() => _cosmosDbOptions.CosmosContainerOptions.All(options => ContainerIds.Contains(options.ContainerId));
    }
}