using System.Text.Json.Serialization;

namespace ActiveCampaign.Models;

public class AccountCustomField
{
    [JsonPropertyName("customFieldId")]
    public int Id { get; set; }

    [JsonPropertyName("fieldValue")]
    public string? Value { get; set; }

    [JsonPropertyName("fieldCurrency")]
    public string? CurrencyType { get; set; }

}