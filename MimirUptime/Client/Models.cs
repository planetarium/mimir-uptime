using System.Text.Json;
using System.Text.Json.Serialization;

namespace NodeUptime.Client;

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

public class ChainQueryResponse
{
    [JsonPropertyName("chainQuery")]
    public ChainQuery? ChainQuery { get; set; }
}

public class ChainQuery
{
    [JsonPropertyName("blockQuery")]
    public BlockQuery? BlockQuery { get; set; }
}

public class BlockQuery
{
    [JsonPropertyName("blocks")]
    public List<Block>? Blocks { get; set; }
}

public class Block
{
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}
