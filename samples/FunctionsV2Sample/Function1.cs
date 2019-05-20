using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FunctionsV2Sample
{
    public class Function1
    {
        private readonly ICarRepository _repository;

        public Function1(ICarRepository carRepository)
        {
            _repository = carRepository;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request.(METHOD:{req.Method})");

            var car = CreateSampleCarInstance();
            await _repository.RegisterAsync(car);

            var target = await _repository.FindAsync(car.AgencyId, car.Id);

            return new OkObjectResult(target);
        }

        private static Car CreateSampleCarInstance()
        {
            return new Car()
            {
                Id = Guid.NewGuid().ToString(),
                CarCategory = $"Category{Random.Next(1, 3)}",
                Description = "Hello world",
                AgencyId = $"Agency{Random.Next(1, 3)}",
                StaffId = $"Staff{Random.Next(1, 100):000}",
            };
        }

        private static readonly Random Random = new Random();
    }
}