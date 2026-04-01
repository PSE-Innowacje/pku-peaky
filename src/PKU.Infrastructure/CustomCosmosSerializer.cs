using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PKU.Infrastructure;

public class CustomCosmosSerializer : CosmosSerializer
{
    private readonly JsonSerializerSettings _settings;

    public CustomCosmosSerializer()
    {
        _settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            },
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    public override T FromStream<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        var serializer = JsonSerializer.Create(_settings);
        return serializer.Deserialize<T>(jsonReader)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        using var jsonWriter = new JsonTextWriter(writer);
        var serializer = JsonSerializer.Create(_settings);
        serializer.Serialize(jsonWriter, input);
        jsonWriter.Flush();
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
