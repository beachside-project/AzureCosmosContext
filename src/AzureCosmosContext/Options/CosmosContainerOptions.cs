using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace AzureCosmosContext.Options
{
    public class CosmosContainerOptions
    {
        public string ContainerId { get; set; }

        public string PartitionKey { get; set; }

        /// <summary>
        /// UniqueKey にしていするパスのCollection
        /// </summary>
        /// <remarks>
        /// CosmosDBの制約で、文字列の先頭は "/" で始まる必要があります。
        /// </remarks>
        public Collection<string> UniqueKeys { get; set; }


        public ContainerProperties ToContainerProperties()
        {
            var properties = new ContainerProperties(ContainerId, PartitionKey);

            if (UniqueKeys != null && UniqueKeys.Any())
            {
                var cosmosUniqueKey = new UniqueKey();

                foreach (var key in UniqueKeys)
                {
                    cosmosUniqueKey.Paths.Add(key);
                }

                properties.UniqueKeyPolicy.UniqueKeys.Add(cosmosUniqueKey);
            }

            // TODO: set IndexingMode
            //properties.IndexingPolicy.IndexingMode = IndexingMode.Consistent;

            return properties;
        }
    }
}