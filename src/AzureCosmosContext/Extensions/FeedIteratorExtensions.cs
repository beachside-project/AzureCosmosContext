using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureCosmosContext.Extensions
{
    public static class FeedIteratorExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this FeedIterator<T> feedIterator)
        {
            var items = new List<T>();

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();
                items.AddRange(feedResponse);
            }

            return items;
        }
    }
}