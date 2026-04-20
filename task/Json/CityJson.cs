using System.Text.Json.Serialization;

namespace task.Json;

public class CityJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("cityID")]
    public long CityId { get; set; }

    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }

    [JsonPropertyName("timeshift")]
    public string? TimeShift { get; set; }

    [JsonPropertyName("requestEndTime")]
    public string? RequestEndTime { get; set; }

    [JsonPropertyName("day2dayRequest")]
    public string? Day2DayRequest { get; set; }

    [JsonPropertyName("freeStorageDays")]
    public string? FreeStorageDays { get; set; }

    [JsonPropertyName("terminals")]
    public List<TerminalJson> Terminals { get; set; } = [];
}
