using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCosmosContext.ConsoleAppSample
{
    public class Car
    {
        public string Id { get; set; }

        public string CarCategory { get; set; }

        public string Description { get; set; }
        public string EngineType { get; set; }
    }
}