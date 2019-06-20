using Microsoft.Azure.Cosmos;

namespace AzureCosmosContext.Extensions
{
    public static class QueryRequestOptionsExtensions
    {
        public static QueryRequestOptions AddPartitionKey(this QueryRequestOptions options, object partitionKey)
        {
            if (partitionKey == null) return options;

            options.PartitionKey = new PartitionKey(partitionKey);
            return options;
        }
    }
}