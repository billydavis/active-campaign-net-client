using System.Text.Json.Serialization;

namespace ActiveCampaign.Models;

// Built in types. I assume these don't change, but who knows!
public enum AccountFieldsEnum
{
    Description = 1,
    Address1 = 2,
    Address2 = 3,
    City = 4,
    State = 5,
    PostalCode = 6,
    Country = 7,
    EmployeeCount = 8,
    AnnualRevenue = 9,
    Industry = 10,
    Phone = 11
}

public class Account : IActiveCampaignEntity
{
    public string? Id { get; set; } = null;

    public string? Name { get; set; } = null;

    public string? AccountUrl { get; set; } = null;

    [JsonPropertyName("owner")]
    public string? OwnerId { get; set; } = null; // Defaults to 1 if not provided.

    public Dictionary<string, string> Links { get; set; } = new();

    public List<AccountCustomField> Fields { get; set; } = new();

    public Account AddOrUpdateField(AccountFieldsEnum field, string value, string? currency = null)
    {
        return AddOrUpdateField((int)field, value, currency);
    }

    public Account AddOrUpdateField(int field, string value, string? currency = null)
    {
        var existingField = Fields.FirstOrDefault(f => f.Id == field);
        if (existingField != null)
        {
            existingField.Value = value;
            existingField.CurrencyType = currency;
        }
        else
        {
            Fields.Add(new AccountCustomField { Id = field, Value = value, CurrencyType = currency });
        }

        return this;
    }
}

public struct AccountResult
{
    public Account Account { get; set; }
}

public struct AccountUpdate
{
   
    public IEnumerable<AccountCustomField> Fields { get; set; }
}

public struct AccountList
{
    public IEnumerable<Account> Accounts { get; set; }
    public MetaData Meta { get; set; }
}