using System.Text.Json.Serialization;

namespace task.Json;

public class PhoneJson
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = null!;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("primary")]
    public string? Primary { get; set; }
}
