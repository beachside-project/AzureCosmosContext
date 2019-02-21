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

        public CosmosContainerSettings ToCosmosContainerSettings()
        {
            var settings = new CosmosContainerSettings(ContainerId, PartitionKey);
            if (UniqueKeys != null && UniqueKeys.Any())
            {
                settings.UniqueKeyPolicy = new UniqueKeyPolicy
                {
                    UniqueKeys = new Collection<UniqueKey>
                    {
                        new UniqueKey
                        {
                            Paths = UniqueKeys
                        }
                    }
                };
            }

            return settings;
        }
    }
}