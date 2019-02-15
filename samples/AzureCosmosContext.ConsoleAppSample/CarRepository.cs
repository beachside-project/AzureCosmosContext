using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureCosmosContext.ConsoleAppSample
{
    public interface ICarRepository
    {
        Task<Car> GetCarAsync(string partitionKey, string id);

        Task RegisterCarAsync(Car car);

        Task UpdateCarAsync(Car car);

        Task<IEnumerable<Car>> GetCarsByCarCategoryAsync(string carCategory);
    }

    public class CarRepository : CosmosRepositoryCore, ICarRepository
    {
        private readonly ILogger _logger;
        public override string ContainerId => "carContainer";

        public CarRepository(CosmosContext context, ILogger<CarRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<Car> GetCarAsync(string partitionKey, string id)
        {
            return await GetItemByIdAsync<Car>(partitionKey, id);
        }

        public async Task RegisterCarAsync(Car car)
        {
            await CreateItemAsync(car.CarCategory, car);
        }

        public async Task UpdateCarAsync(Car car)
        {
            await UpdateItemAsync(car.CarCategory, car.Id, car);
        }

        public async Task<IEnumerable<Car>> GetCarsByCarCategoryAsync(string carCategory)
        {
            var query = new CosmosSqlQueryDefinition("select * from cars c where c.carCategory = @carCategory ")
                .UseParameter("@carCategory", carCategory);

            return await GetItemsAsync<Car>(query);
        }
    }
}