using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppSample
{
    public class SerializerSample : IHostedService
    {
        private readonly IItemRepository _repository;
        private readonly ILogger<SerializerSample> _logger;

        public SerializerSample(IItemRepository repository, ILogger<SerializerSample> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var item = new Item("103", "Div0", "Company1", "Employee2");
                await _repository.RegisterAsync(item);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}