using System.Threading.Tasks;
using AzureCosmosContext;
using Microsoft.Extensions.Logging;

namespace ConsoleAppSample
{
    public interface IItemRepository
    {
        Task<Item> FindAsync(string partitionKey, string id);
        Task RegisterAsync(Item item);
    }


    public class ItemRepository : CosmosRepositoryCore, IItemRepository
    {
        private readonly ILogger<CosmosRepositoryCore> _logger;
        public override string ContainerId => "items";

        public ItemRepository(CosmosContext context, ILogger<CosmosRepositoryCore> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<Item> FindAsync(string partitionKey, string id)
        {
            return await GetItemByIdAsync<Item>(partitionKey, id);
        }

        public async Task RegisterAsync(Item item)
        {
            await CreateItemAsync(item.Division, item);
        }
    }
}