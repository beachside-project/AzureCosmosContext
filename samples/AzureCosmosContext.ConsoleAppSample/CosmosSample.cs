using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureCosmosContext.ConsoleAppSample
{
    public class CosmosSample : IHostedService
    {
        private readonly ICarRepository _repository;
        private readonly ILogger _logger;

        public CosmosSample(ICarRepository repository, ILogger<CosmosSample> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 5; i++)
            {
                // create car
                var newCar = CreateSampleCarInstance();
                await _repository.RegisterCarAsync(newCar);

                // update car
                newCar.Description = $"updated: {DateTime.Now}";
                await _repository.UpdateCarAsync(newCar);
            }

            // get cars
            var targetCars = await _repository.GetCarsByCarCategoryAsync("Category2");

            _logger.LogInformation($"get cars of Category2...");
            foreach (var car in targetCars)
            {
                _logger.LogInformation($"{car.Id}: {car.CarCategory}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("executed constructor");
            return Task.CompletedTask;
        }

        private static Car CreateSampleCarInstance()
        {
            return new Car()
            {
                Id = Random.Next(1, 100).ToString("000"),
                CarCategory = $"Category{Random.Next(1, 5)}",
                Description = "Hello world",
                EngineType = $"Type{Random.Next(1, 100):000}"
            };
        }

        private static readonly Random Random = new Random();
    }
}