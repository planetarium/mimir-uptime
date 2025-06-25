using System.Text.Json;
using System.Text.Json.Serialization;

namespace MimirUptime.Client;

public class GraphQLRequest
{
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    [JsonPropertyName("variables")]
    public object? Variables { get; set; }
}

public class GraphQLResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public JsonElement[]? Errors { get; set; }
}

public class GetTipResponse
{
    [JsonPropertyName("nodeStatus")]
    public required NodeStatus NodeStatus { get; set; }
}

public class NodeStatus
{
    [JsonPropertyName("tip")]
    public required Tip Tip { get; set; }
}

public class Tip
{
    [JsonPropertyName("index")]
    public long Index { get; set; }
}
