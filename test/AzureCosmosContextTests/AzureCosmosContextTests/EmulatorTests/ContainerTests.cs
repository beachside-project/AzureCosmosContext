using AzureCosmosContext;
using AzureCosmosContextTests.Helpers;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AzureCosmosContextTests.EmulatorTests
{
    public class ContainerTests : TestConfigurationBase, IDisposable
    {
        #region setup

        private ServiceProvider _serviceProvider;
        private CosmosContext _cosmosContext;

        public ContainerTests()
        {
            _serviceProvider = ServiceCollection.BuildServiceProvider();
            _cosmosContext = _serviceProvider.GetService<CosmosContext>();
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cosmosContext = null;
                _serviceProvider = null;
            }

            _disposed = true;
            base.Dispose(disposing);
        }

        #endregion setup

        [Fact]
        public async Task DatabaseNameShouldSetCorrectly()
        {
            DatabaseResponse databaseResponse = await _cosmosContext.Database.ReadAsync();
            DatabaseProperties properties = databaseResponse;
            var actual = properties.Id;

            actual.Should().Be("EmulatorTestDb");
        }

        [Fact]
        public async Task DatabaseThroughputShouldSetCorrectly()
        {
            var actual = await _cosmosContext.Database.ReadThroughputAsync();

            actual.Should().Be(500);
        }

        [Fact]
        public async Task ContainerShouldSetupCorrectly()
        {
            var containersCount = _cosmosContext.Containers.Count;
            var existsCarContainer = _cosmosContext.Containers.TryGetValue("car", out var carContainer);
            var existsItemContainer = _cosmosContext.Containers.TryGetValue("item", out var itemContainer);

            // container assertion
            containersCount.Should().Be(2);
            existsCarContainer.Should().BeTrue();
            existsItemContainer.Should().BeTrue();

            // car container assertion
            ContainerProperties carProperties = await carContainer.ReadContainerAsync();

            carProperties.PartitionKeyPath.Should().Be("/carCategory");
            carProperties.UniqueKeyPolicy.UniqueKeys.Count.Should().Be(0);

            // item container assertion
            ContainerProperties itemProperties = await itemContainer.ReadContainerAsync();

            itemProperties.PartitionKeyPath.Should().Be("/color");
            var itemUniqueKey = itemProperties.UniqueKeyPolicy.UniqueKeys.First().Paths.First();
            itemUniqueKey.Should().Be("/category/id");
        }
    }
}