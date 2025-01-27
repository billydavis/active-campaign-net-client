using Newtonsoft.Json;

namespace ActiveCampaign;

public struct TagData
{
    [JsonProperty("tagType")] public string TagType;
    [JsonProperty("tag")] public string Tag;
    [JsonProperty("description")] public string Description;
    [JsonProperty("id")] public int Id;
    [JsonProperty("subscriber_count")] public int SubscriberCount;
}
