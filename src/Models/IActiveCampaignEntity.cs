using System.Text.Json.Serialization;

namespace ActiveCampaign.Models;

public interface IActiveCampaignEntity
{
    public string? Id { get; set; }

    public Dictionary<string, string> Links { get; set; }
}