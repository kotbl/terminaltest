using System.Text.Json;
using System.Text.Json.Serialization;

namespace task.Json;

public class TerminalJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("fullAddress")]
    public string? FullAddress { get; set; }

    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }

    [JsonPropertyName("isPVZ")]
    public string? IsPVZ { get; set; }

    [JsonPropertyName("cashOnDelivery")]
    public string? CashOnDelivery { get; set; }

    [JsonPropertyName("storage")]
    public string? Storage { get; set; }

    [JsonPropertyName("receiveCargo")]
    public string? ReceiveCargo { get; set; }

    [JsonPropertyName("giveoutCargo")]
    public string? GiveoutCargo { get; set; }

    [JsonPropertyName("phones")]
    public List<PhoneJson> Phones { get; set; } = [];

    [JsonPropertyName("maxWeight")]
    public JsonElement? MaxWeight { get; set; }

    [JsonPropertyName("maxLength")]
    public JsonElement? MaxLength { get; set; }

    [JsonPropertyName("maxWidth")]
    public JsonElement? MaxWidth { get; set; }

    [JsonPropertyName("maxHeight")]
    public JsonElement? MaxHeight { get; set; }

    [JsonPropertyName("worktables")]
    public JsonElement? Worktables { get; set; }
}
