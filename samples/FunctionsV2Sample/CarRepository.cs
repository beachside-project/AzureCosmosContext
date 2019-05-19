using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCosmosContext;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace FunctionsV2Sample
{
    public class CarRepository : CosmosRepositoryCore, ICarRepository
    {
        private readonly ILogger _logger;
        public override string ContainerId => "cars";

        public CarRepository(CosmosContext context, ILogger<CarRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<Car> FindAsync(string partitionKey, string id)
        {
            return await GetItemByIdAsync<Car>(partitionKey, id);
        }

        public async Task RegisterAsync(Car car)
        {
            await CreateItemAsync(car.AgencyId, car);
            _logger.LogTrace($"registered car. (ID: {car.Id}; Agency: {car.AgencyId};)");

        }

        public async Task UpdateAsync(Car car)
        {
            await UpdateItemAsync(car.AgencyId, car.Id, car);
        }

        public async Task<IEnumerable<Car>> GetCarsByCarCategoryAsync(string carCategory)
        {
            var query = new CosmosSqlQueryDefinition("select * from cars c where c.carCategory = @carCategory ")
                .UseParameter("@carCategory", carCategory);

            return await GetItemsAsync<Car>(query);
        }
    }
}