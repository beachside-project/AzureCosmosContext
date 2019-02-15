using Microsoft.Azure.Cosmos;

namespace AzureCosmosContext.Options
{
    public class CosmosContainerOptions
    {
        public string ContainerId { get; set; }
        public string PartitionKey { get; set; }

        public CosmosContainerSettings ToCosmosContainerSettings() => new CosmosContainerSettings(ContainerId, PartitionKey);
    }
}