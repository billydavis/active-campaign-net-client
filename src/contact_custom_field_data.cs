using Newtonsoft.Json;

namespace ActiveCampaign;

public struct ContactCustomFieldData
{
    [JsonProperty("contact")] public int Contact;
    [JsonProperty("id")] public int Id;
    [JsonProperty("field")] public int FieldId;
    [JsonProperty("value")] public string Value;
    [JsonProperty("cdate")] public DateTime CreationDate;
    [JsonProperty("udate")] public DateTime UpdateDate;

}
