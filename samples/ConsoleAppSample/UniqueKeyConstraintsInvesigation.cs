using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppSample
{
    public class UniqueKeyConstraintsInvestigation : IHostedService
    {
        private readonly IItemRepository _repository;
        private readonly ILogger<UniqueKeyConstraintsInvestigation> _logger;

        public UniqueKeyConstraintsInvestigation(IItemRepository repository, ILogger<UniqueKeyConstraintsInvestigation> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("done");
            return Task.CompletedTask;
        }

        private async Task RunAsync()
        {
            var baseItem = new Item("100", "Div0", "Company0", "Employee0");
            await _repository.RegisterAsync(baseItem);

            var item1 = new Item("101", "Div0", "Company1", "Employee1");
            await TryAsync(1, item1);

            var item2 = new Item("102", "Div0", "Company0", "Employee0");
            await TryAsync(2, item2);

            var item3 = new Item("103", "Div3", "Company3", "Employee3");
            await TryAsync(3, item3);

            var item4 = new Item("104", "Div4", "Company0", "Employee0");
            await TryAsync(4, item4);

            var item5 = new Item("100", "Div0", "Company5", "Employee5");
            await TryAsync(5, item5);

            var item6 = new Item("100", "Div0", "Company0", "Employee0");
            await TryAsync(6, item6);

            var item7 = new Item("100", "Div7", "Company7", "Employee7");
            await TryAsync(7, item7);

            var item8 = new Item("100", "Div8", "Company0", "Employee0");
            await TryAsync(8, item8);
        }

        public async Task TryAsync(int patternId, Item item)
        {
            try
            {
                _logger.LogInformation($"Pattern {patternId}:::::::::::::::::::::::::");
                await _repository.RegisterAsync(item);
                _logger.LogInformation("Succeeded");
            }
            catch (CosmosException ce)
            {
                _logger.LogError($"Error!!!!!!!!{ce.StatusCode}");
                _logger.LogDebug($"item: {JsonConvert.SerializeObject(item)}");
                _logger.LogDebug(ce.Message);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }
    }
}