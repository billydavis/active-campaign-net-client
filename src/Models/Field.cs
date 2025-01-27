using System.Text.Json.Serialization;

namespace ActiveCampaign.Models;

public struct Field
{
    [JsonPropertyName("contact")]
    public string? Contact { get; set; }

    [JsonPropertyName("field")]
    public int FieldId { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}