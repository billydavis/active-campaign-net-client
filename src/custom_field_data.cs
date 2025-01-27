using Newtonsoft.Json;

namespace ActiveCampaign;

public struct CustomFieldData
{
    [JsonProperty("title")] public string Title;
    [JsonProperty("descript")] public string Description;
    [JsonProperty("type")] public string Type;
    [JsonProperty("perstag")] public string PersistentTag;
    [JsonProperty("id")] public int Id;
}
