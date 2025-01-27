using Newtonsoft.Json;

namespace ActiveCampaign;

public class ListData
{
    [JsonProperty("stringid")] public string StringId;
    [JsonProperty("userid")] public int UserId;
    [JsonProperty("name")] public string Name;
    [JsonProperty("id")] public int Id;
    [JsonProperty("cdate")] public DateTime CreationDate;
}
