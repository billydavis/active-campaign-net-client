namespace ActiveCampaign.Models;

public struct Contact : IActiveCampaignEntity
{
    public Contact()
    {
        Id = null;
        FirstName = null;
        LastName = null;
        Email = null;
        Phone = null;
        CreatedUtcTimeStamp = null;
        UpdatedUtcTimeStamp = null;
        Links = new Dictionary<string, string>();
    }
    
    public string? Id { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    
    public DateTime? CreatedUtcTimeStamp;
    
    public DateTime? UpdatedUtcTimeStamp;

    public Dictionary<string, string> Links { get; set; }

    // [JsonPropertyName("fieldValues")]
    // public IEnumerable<Field> FieldValues { get; set; } = new List<Field>();
}

public struct ContactResult
{
    public Contact Contact { get; set; }
}

public struct ContactList
{
    public IEnumerable<Contact> Contacts { get; set; }
    public MetaData Meta { get; set; }
}
