using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AspnetCoreSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ICarRepository _repository;
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ICarRepository repository, ILogger<ValuesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("called ValuesController.Get method....");
            var car = CreateSampleCarInstance();
            await _repository.RegisterAsync(car);

            var target = await _repository.FindAsync(car.CarCategory, car.Id);
            return Ok(target);
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