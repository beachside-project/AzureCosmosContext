using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCosmosContextTests.Helpers
{
    /// <summary>
    /// document schema for item container
    /// </summary>
    /// <remarks>
    /// Item-container settings
    /// - partition key: color
    /// - unique key: category/id
    /// </remarks>
    public class ItemModel
    {
        public string Id { get; set; }
        public string Color { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int Term { get; set; }
    }
}
