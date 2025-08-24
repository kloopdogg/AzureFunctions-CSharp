using System.Text.Json.Serialization;

namespace SampleFunctionApp.Models;

public class SampleInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}