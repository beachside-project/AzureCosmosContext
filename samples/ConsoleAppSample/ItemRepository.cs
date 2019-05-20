using AzureCosmosContext;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleAppSample
{
    public interface IItemRepository
    {
        Task<Item> FindAsync(string partitionKey, string id);

        Task RegisterAsync(Item item);

        Task<IEnumerable<Item>> GetItemAllAsync();
    }

    public class ItemRepository : CosmosRepositoryCore, IItemRepository
    {
        private readonly ILogger _logger;
        public override string ContainerId => "items";

        public ItemRepository(CosmosContext context, ILogger<ItemRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task RegisterAsync(Item item)
        {
            await CreateItemAsync(item.Division, item);
            _logger.LogTrace($"Created. (ID:{item.Id})");
        }

        public async Task<Item> FindAsync(string partitionKey, string id)
        {
            return await GetItemByIdAsync<Item>(partitionKey, id);
        }

        public async Task<IEnumerable<Item>> GetItemAllAsync()
        {
            var query = new CosmosSqlQueryDefinition("select * from t ");
            //.UseParameter("@account", "12345");

            return await GetItemsAsync<Item>(query);
        }
    }
}