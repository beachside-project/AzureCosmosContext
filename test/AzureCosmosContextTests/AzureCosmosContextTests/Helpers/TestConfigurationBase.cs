using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using AzureCosmosContext;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AzureCosmosContextTests.Helpers
{
    public abstract class TestConfigurationBase : IDisposable
    {
        public IConfiguration Configuration { get; set; }
        public ServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

        protected TestConfigurationBase()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            //ServiceCollection.AddSingleton<ILogger>(NullLogger.Instance);
            ServiceCollection.AddSingleton<ILogger<CosmosContext>>(NullLogger<CosmosContext>.Instance);

        }

        #region GC

        bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Configuration = null;
                ServiceCollection = null;
                //
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
        }

        ~TestConfigurationBase()
        {
            Dispose(false);
        }
                
        #endregion
    }
}