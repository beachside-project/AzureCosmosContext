using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureCosmosContext;
using AzureCosmosContextTests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace AzureCosmosContextTests.EmulatorTests
{
    public class ItemHandleTests : TestConfigurationBase
    {
        #region setup

        private ServiceProvider _serviceProvider;
        private CosmosContext _cosmosContext;

        public ItemHandleTests()
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

        #endregion

        public async Task ItemCanInsert()
        {

        }
    }
}
