﻿using AzureCosmosContext;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspnetCoreSample
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
            await CreateItemAsync(car.CarCategory, car);
            _logger.LogTrace($"registered car. (ID: {car.Id}; Agency: {car.AgencyId};)");

        }

        public async Task UpdateAsync(Car car)
        {
            await UpdateItemAsync(car.CarCategory, car.Id, car);
        }

        public async Task<IEnumerable<Car>> GetCarsByCarCategoryAsync(string carCategory)
        {
            var query = new QueryDefinition("select * from cars c where c.carCategory = @carCategory ")
                .WithParameter("@carCategory", carCategory);

            return await GetItemsAsync<Car>(query);
        }
    }
}