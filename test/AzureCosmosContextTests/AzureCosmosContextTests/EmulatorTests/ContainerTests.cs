using AzureCosmosContextTests.Helpers;
using Microsoft.Win32.SafeHandles;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AzureCosmosContext;
using AzureCosmosContext.Options;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AzureCosmosContextTests.EmulatorTests
{
    public class ContainerTests : TestConfigurationBase, IDisposable
    {
        #region setup

        private readonly ServiceProvider _serviceProvider;
        private readonly CosmosContext _cosmosContext;

        public ContainerTests() 
        {
            //DI のセットアップ
            ServiceCollection.AddCosmosContext(Configuration);

            // database / container を作成する




            _serviceProvider = ServiceCollection.BuildServiceProvider();
            _cosmosContext = _serviceProvider.GetService<CosmosContext>();

        }

        private bool _disposed;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _handle.Dispose();
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //

            _disposed = true;
            // Call base class implementation.
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
            var throughputResponse = await _cosmosContext.Database.ReadThroughputAsync();
            var actual = throughputResponse.Resource.Throughput;

            actual.Should().Be(500);
        }

        [Fact]
        public async Task ContainerShoudSetupCorrectly()
        {
            var containersCount = _cosmosContext.Containers.Count;
            var existsCarContainer = _cosmosContext.Containers.TryGetValue("car",out var carContainer);
            var existsItemContainer = _cosmosContext.Containers.TryGetValue("item",out var itemContainer);

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