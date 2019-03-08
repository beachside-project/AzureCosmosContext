using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace AzureCosmosContext
{
    public class CustomizableCaseJsonSerializer : CosmosJsonSerializer
    {
        public CustomizableCaseJsonSerializer(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }

        private const int BufferSize = 1024;

        private readonly JsonSerializer _serializer;

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)stream;
                }

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        return _serializer.Deserialize<T>(jsonTextReader);
                    }
                }
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, Encoding.Default, BufferSize, leaveOpen: true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.None;
                    _serializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}