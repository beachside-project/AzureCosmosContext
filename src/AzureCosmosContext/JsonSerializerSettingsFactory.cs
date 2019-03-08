using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AzureCosmosContext
{
    public class JsonSerializerSettingsFactory
    {
        public static JsonSerializerSettings CreateForCamelCase() 
            => new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static JsonSerializerSettings CreateForReadonlyIgnoreAndCamelCase() 
            => new JsonSerializerSettings() { ContractResolver = new ReadOnlyPropertyIgnoranceWithCamelCaseResolver() };
    }
}