using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureCosmosContext.Extensions
{
    public static class TaskExtensions
    {
        public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);
    }
}