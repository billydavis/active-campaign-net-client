using ActiveCampaign.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ActiveCampaign;

public class ContactService
{
    private readonly IClient? _client;

    public ContactService(IClient client)
    {
        _client = client;
    }
    

    public IEnumerable<Contact> Search(ContactStatus status = ContactStatus.Any, SearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return RetrieveContactsAsync(null, status, null, options, cancellationToken).Result;
    }
    
    public Contact Get(int contactId, CancellationToken cancellationToken = default)
    {
        var query = $"/contacts/{contactId}";

        if (_client == null)
            throw new Exception("client is invalid");

        var result = _client.Get<ContactResult>(query, cancellationToken ).Result;

        return result.Contact;
    }

    public Contact Update(int contactId, Contact contact,  CancellationToken cancellationToken = default)
    {
        var endpoint = $"/contacts/{contactId}";

        if (_client == null)
            throw new Exception("client is invalid");

        var jsonData = JsonSerializer.Serialize(new { contact }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        var result = _client.Put<ContactResult>(endpoint, jsonData, cancellationToken).Result;

        return result.Contact;
    }

    public object? UpdateFieldValue(int contactId, int field, string value, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/fieldValues";

        var data = new { fieldValue = new { contact = contactId, field, value } };
        if (_client == null)
            throw new Exception("client is invalid");

        var jsonData = JsonSerializer.Serialize(data);

        // TODO fix this potentially!

        return _client.Post<object>(endpoint, jsonData, cancellationToken).Result;
    }


    private async Task<IEnumerable<Contact>> RetrieveContactsAsync(DateRange? dateRange, ContactStatus status,
        string querySuffix, SearchOptions? options = default, CancellationToken cancellationToken = default)
    {
        const string endpoint = $"/contacts";
        const int limit = 20;

        var dateFilter = "";

        if (dateRange.HasValue)
        {
            var f = dateRange.Value.Start.ToUniversalTime();
            var t = dateRange.Value.End.ToUniversalTime();

            var dateBefore = $"{t.Year}/{t.Month}/{t.Day}T{t.Hour}:{t.Minute}:{t.Second}-00:00";
            var dateAfter = $"{f.Year}/{f.Month}/{f.Day}T{f.Hour}:{f.Minute}:{f.Second}-00:00";

            dateFilter =
                $"&filters[created_before]={Uri.EscapeDataString(dateBefore)}&filters[created_after]={Uri.EscapeDataString(dateAfter)}";
        }

        var offset = 0;
        var totalProcessed = 0;
        var totalExpected = 0;

        var searchContacts = new List<Contact>();

        async Task Process()
        {
            do
            {
                var query = endpoint + $"?limit={limit}&offset={offset}" + dateFilter;
                offset += limit;

                query += $"&status={(int)status}";
                query += querySuffix;
                
                if (_client == null)
                    throw new Exception("client is invalid");

                var results = await _client.Get<ContactList>(query, cancellationToken);
                totalExpected = Convert.ToInt32(results.Meta.Total);
                if (results.Contacts.Any())
                {
                    searchContacts.AddRange(results.Contacts);
                    totalProcessed += results.Contacts.Count();

                    // Send Progress
                    options?.RaiseProcessedEvent(totalProcessed, totalExpected);
                }

                if (results.Contacts.Count() < limit)
                    break;

            } while (true);
        }

        var tasks = new List<Task>();
        
        do
        {
            var task = Process();
            tasks.Add(task);
        } while (totalProcessed != totalExpected);
        
        await Task.WhenAll(tasks);
        
        return searchContacts;
    }


}