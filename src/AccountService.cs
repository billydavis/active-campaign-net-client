using System.Text.Json;
using System.Text.Json.Serialization;
using ActiveCampaign.Models;

namespace ActiveCampaign;

public class AccountService
{
    private readonly Client _client;

    public AccountService(Client client)
    {
        _client = client;
    }
    
    public Account Create(Account account, CancellationToken cancellationToken = default)
    {
        var endpoint = "/accounts";

        if (_client == null)
            throw new Exception("client is invalid");

        var jsonData = JsonSerializer.Serialize(new {account}, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        
        return _client.Post<Account>(endpoint, jsonData, cancellationToken).Result;
    }


    public Account Get(int accountId, CancellationToken cancellationToken = default)
    {
        var query = $"/accounts/{accountId}";

        if (_client == null)
            throw new Exception("client is invalid");

        var result = _client.Get<AccountResult>(query, cancellationToken).Result;

        return result.Account;
    }

    public IEnumerable<Account> Search(string? searchFor = default, bool countDeals = false, SearchOptions? options = default,  CancellationToken cancellationToken = default)
    {
        const string endpoint = $"/accounts";
        const int limit = 20;
        
        var offset = 0;
        var totalProcessed = 0;
        var totalExpected = 0;

        var searchAccounts = new List<Account>();

        async Task Process()
        {
            do
            {
                var query = endpoint + $"?limit={limit}&offset={offset}";
                offset += limit;

                if (!string.IsNullOrEmpty(searchFor))
                    query = $"{query}&search={searchFor}";

                if (_client == null)
                    throw new Exception("client is invalid");

                var results = await _client.Get<AccountList>(query, cancellationToken);
                totalExpected = Convert.ToInt32(results.Meta.Total);
                if (results.Accounts.Any())
                {
                    searchAccounts.AddRange(results.Accounts);
                    totalProcessed += results.Accounts.Count();

                    // Send Progress TODO make generic process options.
                    options?.RaiseProcessedEvent(totalProcessed, totalExpected);
                }

                if (results.Accounts.Count() < limit)
                    break;

            } while (true);
        }

        var tasks = new List<Task>();

        do
        {
            var task = Process();
            tasks.Add(task);
        } while (totalProcessed != totalExpected);

        Task.WhenAll(tasks).Wait(cancellationToken);

        return searchAccounts;

    }

    public Account Update(int accountId, Account account, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/accounts/{accountId}";

        if (_client == null)
            throw new Exception("client is invalid");

        var jsonData = JsonSerializer.Serialize(new { account }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        var result = _client.Put<AccountResult>(endpoint, jsonData, cancellationToken).Result;

        return result.Account; 
    }

    public bool Delete(int accountId, CancellationToken cancellationToken = default)
    {
        var query = $"/accounts/{accountId}";

        if (_client == null)
            throw new Exception("client is invalid");

        var result = _client.Delete(query, cancellationToken).Result;

        return result;
    }
   
}