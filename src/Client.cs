using System.Text;
using System.Text.Json;

namespace ActiveCampaign;

public class Client : IClient, IDisposable
{
    private readonly HttpClient _httpClient = new();
    private readonly string _url;

    // ActiveCampaign imposes a limit of 5 requests per second
    private static readonly ResourceGuard ActivityLimiter = new (5, 1000);

    public Client(string url, string key)
    {
        _url = url + "/api/3"; // TODO maybe put this into a builder function.
        _httpClient.DefaultRequestHeaders.Add("Api-Token", key);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient.Dispose();
        }
    } 

    /*
    public Task<CampaignData[]> GetAllCampaignsAsync(Action<int, int> onProgressCallback = null, CancellationToken cancellationToken = default)
    {
        return ListAllElementsAsync<CampaignData, GetCampaignsDataResponse>("campaigns", onProgressCallback, cancellationToken);
    }

    public async Task<CampaignData> GetCampaignById(int campaignId, CancellationToken cancellationToken = default)
    {
        var query = _url + $"/campaigns/{campaignId}";

        using var result = await DoGetAsync(query, cancellationToken);

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var data = JsonConvert.DeserializeObject<CampaignResult>(jsonData);
        return data.Campaign;
    }

    public async Task<ContactListStatus[]> GetContactListsStatusAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var query = _url + $"/contacts/{contactId}/contactLists";

        using var result = await DoGetAsync(query, cancellationToken);

        result.EnsureSuccessStatusCode();

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var ldr = JsonConvert.DeserializeObject<ContactListStatusResponse>(jsonData, _jsonSerializerSettings);

        return ldr.contactLists;
    }

    public async Task<ContactListStatus> UpdateContactListStatusAsync(int contactId, int listId, ContactStatus status, CancellationToken cancellationToken = default)
    {
        var contactList = new
        {
            sourceid = 0,
            list = listId,
            contact = contactId,
            status,
        };

        var query = _url + "/contactLists";
        var content = "{ \"contactList\": " + JsonConvert.SerializeObject(contactList) + "}";

        using var result = await DoPostAsync(query, content, cancellationToken);

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        if (!result.IsSuccessStatusCode)
            throw new UpdateContactListStatusAsync(contactId, listId, status, result.StatusCode, result.ReasonPhrase ?? "");

        var r = JsonConvert.DeserializeObject<UpdateContactResponse>(jsonData, _jsonSerializerSettings);

        return r.contactList;
    } 
    */

    public T? Deserialize<T>(string jsonData)
    {
        var result = JsonSerializer.Deserialize<T>(jsonData, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        return result;
    }

    public async Task<T?> Get<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        var path = _url + endpoint;
        try
        {
            var response = await _httpClient.GetAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            var jsonData = await response.Content.ReadAsStringAsync(cancellationToken);

            return Deserialize<T>(jsonData);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Thread.Sleep(1000);
            return default(T);
        }
    }
   

    public async Task<T?> Post<T>(string endpoint, string jsonData, CancellationToken cancellationToken = default)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        var path = _url + endpoint;
        var content = (!string.IsNullOrEmpty(jsonData))
            ? new StringContent(jsonData, Encoding.UTF8, "application/json")
            : null;

        try
        {
            var response = await _httpClient.PostAsync(path, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

            return Deserialize<T>(responseData);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Thread.Sleep(1000);
            return default(T);
        }
    }

    public async Task<T?> Put<T>(string endpoint, string jsonData, CancellationToken cancellationToken = default)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        var path = _url + endpoint;
        var content = (!string.IsNullOrEmpty(jsonData))
            ? new StringContent(jsonData, Encoding.UTF8, "application/json")
            : null;

        try
        {
            var response = await _httpClient.PutAsync(path, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

            return Deserialize<T>(responseData);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Thread.Sleep(1000);
            return default(T);
        }
    }

    public async Task<bool> Delete(string endpoint, CancellationToken cancellationToken)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        var path = _url + endpoint;
        try
        {
            var response = await _httpClient.DeleteAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Thread.Sleep(1000);
            return false;
        }
    }

    /*
    public async Task<ListData?> GetListIdAsync(string listName, CancellationToken cancellationToken = default)
    {
        var query = _url + "/lists?filters[name]=" + Uri.EscapeDataString(listName);

        using var result = await DoGetAsync(query, cancellationToken);

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var ldr = JsonConvert.DeserializeObject<ListsDataResponse>(jsonData);

        if (ldr.meta.total >= 1)
            return ldr.lists[0];

        return null;
    }
    */

    // public async Task<ContactData> GetContactByIdAsync(int contactId, CancellationToken cancellationToken = default)
    // {
    //     var query = _url + $"/contacts/{contactId}";
    //
    //     using var result = await DoGetAsync(query, cancellationToken);
    //
    //     var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);
    //
    //     var data = JsonConvert.DeserializeObject<ContactResult>(jsonData);
    //
    //     return data.Contact;
    // }
    //
    //
    // public async Task<IEnumerable<Contact>> GetContacts(ContactStatus status = ContactStatus.Any, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    // {
    //     return await RetrieveContactsAsync(null, status, "", options, cancellationToken);
    // }

    /*
    public async Task<ContactData[]> GetContactsByListIdAsync(int listId, ContactStatus status, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return await RetrieveContactsAsync(null, status, $"&listid={listId}", options, cancellationToken);
    }

    public async Task<ContactData[]> GetContactsByListIdAsync(int listId, DateRange dateRange, ContactStatus status, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return await RetrieveContactsAsync(dateRange, status, $"&listid={listId}", options, cancellationToken);
    }

    public async Task<CustomFieldData[]> ListAllCustomFieldsAsync(System.Action<int, int> onProgressCallback = null, CancellationToken cancellationToken = default)
    {
        return await ListAllElementsAsync<CustomFieldData, GetCustomFieldsResponse>("fields", onProgressCallback, cancellationToken);
    }

    public Task<ContactCustomFieldData[]> RetreiveAllContactsCustomFieldsValuesAsync(CancellationToken cancellationToken = default)
    {
        return RetreiveAllContactsCustomFieldsValuesAsync(null, cancellationToken);
    }

    public async Task<ContactCustomFieldData[]> RetreiveAllContactsCustomFieldsValuesAsync(ActiveCampaign.GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return await ListAllElementsAsync<ContactCustomFieldData, GetAllContactsCustomFieldsResponse>("fieldValues", options != null ? options.RaiseProcessedEvent : null, cancellationToken);
    }

    public async Task<ContactCustomFieldData[]> RetreiveContactCustomFieldsValuesAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var query = _url + $"/contacts/{contactId}/fieldValues";

        using var result = await DoGetAsync(query, cancellationToken);

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var r = JsonConvert.DeserializeObject<GetContactCustomFieldsResponse>(jsonData);

        return r.fieldValues;
    }

    public Task<ListData[]> ListAllListsAsync(System.Action<int, int> onProgressCallback = null, CancellationToken cancellationToken = default)
    {
        return ListAllElementsAsync<ListData, ListsDataResponse>("lists", onProgressCallback, cancellationToken);
    }

    public Task<TagData[]> ListAllTagsAsync(System.Action<int, int> onProgressCallback = null, CancellationToken cancellationToken = default)
    {
        return ListAllElementsAsync<TagData, TagsDataResponse>("tags", onProgressCallback, cancellationToken);
    }

    public async Task<TagData?> GetTagIdAsync(string tagName, CancellationToken cancellationToken = default)
    {
        var query = _url + "/tags?search=" + Uri.EscapeDataString(tagName);

        using var result = await DoGetAsync(query, cancellationToken);

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var tdr = JsonConvert.DeserializeObject<TagsDataResponse>(jsonData);

        if (tdr.meta.total == 0)
            return null;

        return tdr.tags[0];
    }

    public Task<int> GetContactsCountByTagIdAsync(int tagId, ContactStatus status, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return GetContactsCountByTagIdAsync(tagId, status, null, options, cancellationToken);
    }

    public Task<int> GetContactsCountByTagIdAsync(int tagId, ContactStatus status, DateRange? dateRange, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return RetrieveContactsCountAsync(dateRange, status, $"&tagid={tagId}", options, cancellationToken);
    }

    public async Task<ContactData[]> GetContactsByTagIdAsync(int tagId, ContactStatus status, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return await GetContactsByTagIdAsync(tagId, status, null, options, cancellationToken);
    }

    public async Task<ContactData[]> GetContactsByTagIdAsync(int tagId, ContactStatus status, DateRange? dateRange, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        return await RetrieveContactsAsync(dateRange, status, $"&tagid={tagId}", options, cancellationToken);
    }

    public async Task<int[]> GetContactTagsAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var query = $"{_url}/contacts/{contactId}/contactTags";

        using var result = await DoGetAsync(query, cancellationToken);

        //if( !result.IsSuccessStatusCode )
        //    throw new AddContactException( emailAddress, result.StatusCode, result.ReasonPhrase ?? "" );

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var o = JsonConvert.DeserializeObject<GetContactTagsResponse>(jsonData);

        if (o.contactTags.Length == 0)
            return new int[0];

        var output = new int[o.contactTags.Length];

        for (var i = 0; i < o.contactTags.Length; ++i)
        {
            var c = o.contactTags[i];

            output[i] = c.tag;
        }

        return output;
    }

    public async Task<ContactData> AddContactAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        var contact = new
        {
            email = emailAddress,
        };

        var query = _url + "/contacts";
        var content = "{ \"contact\": " + JsonConvert.SerializeObject(contact) + "}";

        using var result = await DoPostAsync(query, content, cancellationToken);

        if (!result.IsSuccessStatusCode)
            throw new AddContactException(emailAddress, result.StatusCode, result.ReasonPhrase ?? "");

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var acr = JsonConvert.DeserializeObject<AddOrSyncContactResponse>(jsonData);

        return acr.contact;
    }

    public async Task DeleteContactAsync(int contactId, CancellationToken cancellationToken = default)
    {
        var query = $"{_url}/contacts/{contactId}";

        using var result = await DoDeleteAsync(query, cancellationToken);

        //if( !removeResult.IsSuccessStatusCode )
        //    throw new RemoveTagAssociationFromContactException( id, tagId, ta.id, removeResult.StatusCode, removeResult.ReasonPhrase ?? "" );

    }

    public async Task<ContactData> SyncContactDataAsync(string emailAddress, ContactInputData data, CancellationToken cancellationToken = default)
    {
        var firstName = data.FirstName != null ? $"\"firstName\":\"{data.FirstName}\"{(data.LastName != null ? "," : "")}" : "";
        var lastName = data.LastName != null ? $"\"lastName\":\"{data.LastName}\"{(data.Phone != null ? "," : "")}" : "";
        var phone = data.Phone != null ? $"\"phone\":\"{data.Phone}\"{(data.FieldValues != null ? "," : "")}" : "";
        var fieldValues = data.FieldValues != null ? $"\"fieldValues\":{JsonConvert.SerializeObject(data.FieldValues)}" : "";

        var content = $"{{ \"contact\": {{ \"email\":\"{emailAddress}\",{firstName}{lastName}{phone}{fieldValues} }} }}";

        var query = _url + "/contact/sync";

        using var result = await DoPostAsync(query, content, cancellationToken);

        if (!result.IsSuccessStatusCode)
            throw new SyncContactDataException(emailAddress, data, result.StatusCode, result.ReasonPhrase ?? "");

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var acr = JsonConvert.DeserializeObject<AddOrSyncContactResponse>(jsonData);

        return acr.contact;
    }

    public async Task<ContactData[]> SearchContactByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        var query = _url + "/contacts?email=" + Uri.EscapeDataString(emailAddress);

        using var result = await DoGetAsync(query, cancellationToken);

        if (!result.IsSuccessStatusCode)
            throw new SearchContactByEmailAddressException(emailAddress, result.StatusCode, result.ReasonPhrase ?? "");

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var cdr = JsonConvert.DeserializeObject<ContactDataResponse>(jsonData);

        return cdr.contacts;
    }

    public async Task AddTagToContactAsync(int id, int tagId, CancellationToken cancellationToken = default)
    {
        var contactTag = new
        {
            contact = id,
            tag = tagId,
        };

        var query = _url + "/contactTags";
        var content = "{ \"contactTag\": " + JsonConvert.SerializeObject(contactTag) + "}";

        using var result = await DoPostAsync(query, content, cancellationToken);

        if (!result.IsSuccessStatusCode)
            throw new AddTagToContactException(id, tagId, result.StatusCode, result.ReasonPhrase ?? "");
    }

    public async Task<bool> RemoveTagFromContactAsync(int id, int tagId, CancellationToken cancellationToken = default)
    {
        var contactTag = new
        {
            contact = id,
            tag = tagId,
        };

        var getTagAssociationIdQuery = _url + $"/contacts/{id}/contactTags";

        using var listResult = await DoGetAsync(getTagAssociationIdQuery, cancellationToken);

        if (!listResult.IsSuccessStatusCode)
            throw new ListTagAssociationException(id, listResult.StatusCode, listResult.ReasonPhrase ?? "");

        var jsonData = await listResult.Content.ReadAsStringAsync(cancellationToken);

        var gctdr = JsonConvert.DeserializeObject<GetContactTagsDataResponse>(jsonData);

        var index = Array.FindIndex(gctdr.contactTags, p => p.tag == tagId);

        if (index == -1)
            return false;

        var ta = gctdr.contactTags[index];

        var removeTagAssociationQuery = _url + $"/contactTags/{ta.id}";

        using var removeResult = await DoDeleteAsync(removeTagAssociationQuery, cancellationToken);

        if (!removeResult.IsSuccessStatusCode)
            throw new RemoveTagAssociationFromContactException(id, tagId, ta.id, removeResult.StatusCode, removeResult.ReasonPhrase ?? "");

        return true;
    }

    private async Task<TElement[]> ListAllElementsAsync<TElement, TResponseData>(string queryTag, System.Action<int, int> onProgressCallback, CancellationToken cancellationToken = default) where TResponseData : IResponseListData<TElement>
    {
        const int limit = 100;

        var offset = 0;
        var totalProcessed = 0;

        TElement[]? o = null;

        async Task Process()
        {
            do
            {
                var myOffset = offset;
                offset += limit;

                var query = $"{_url}/{queryTag}?offset={myOffset}&limit={limit}";

                System.Diagnostics.Debug.WriteLine(query);

                using var result = await DoGetAsync(query, cancellationToken);

                result.EnsureSuccessStatusCode();

                var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

                var response = JsonConvert.DeserializeObject<TResponseData>(jsonData);

                if (response == null || response.Elements.Length == 0)
                    break;

                if (o == null) o = new TElement[response.Total];

                System.Array.Copy(response.Elements, 0, o, myOffset, response.Elements.Length);

                totalProcessed += response.Elements.Length;

                onProgressCallback?.Invoke(totalProcessed, response.Total);

                if (response.Elements.Length < limit)
                    break;

            } while (true);
        }

        var tasks = new List<Task>();

        for (var i = 0; i < 6; ++i)
        {
            var task = Process();
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        if (o == null)
            return new TElement[0];

        return o;
    }

    private async Task<int> RetrieveContactsCountAsync(DateRange? dateRange, ContactStatus status, string querySuffix, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        var dateFilter = "";

        if (dateRange.HasValue)
        {
            var f = dateRange.Value.Start.ToUniversalTime();
            var t = dateRange.Value.End.ToUniversalTime();

            var dateBefore = $"{t.Year}/{t.Month}/{t.Day}T{t.Hour}:{t.Minute}:{t.Second}-00:00";
            var dateAfter = $"{f.Year}/{f.Month}/{f.Day}T{f.Hour}:{f.Minute}:{f.Second}-00:00";

            dateFilter = $"?filters[created_before]={Uri.EscapeDataString(dateBefore)}&filters[created_after]={Uri.EscapeDataString(dateAfter)}";
        }

        var query = _url + $"/contacts{dateFilter}";

        query += $"&status={(int)status}";
        query += querySuffix;

        using var result = await DoGetAsync(query, cancellationToken);

        result.EnsureSuccessStatusCode();

        var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

        var cdr = JsonConvert.DeserializeObject<ContactDataResponse>(jsonData);

        return cdr.meta.total;
    }

    private async Task<ContactData[]> RetrieveContactsAsync(DateRange? dateRange, ContactStatus status, string querySuffix, GetContactsOptions options = null, CancellationToken cancellationToken = default)
    {
        const int limit = 100;

        var dateFilter = "";

        if (dateRange.HasValue)
        {
            var f = dateRange.Value.Start.ToUniversalTime();
            var t = dateRange.Value.End.ToUniversalTime();

            var dateBefore = $"{t.Year}/{t.Month}/{t.Day}T{t.Hour}:{t.Minute}:{t.Second}-00:00";
            var dateAfter = $"{f.Year}/{f.Month}/{f.Day}T{f.Hour}:{f.Minute}:{f.Second}-00:00";

            dateFilter = $"&filters[created_before]={Uri.EscapeDataString(dateBefore)}&filters[created_after]={Uri.EscapeDataString(dateAfter)}";
        }

        var offset = 0;
        var totalProcessed = 0;

        var o = new List<ContactData>();

        async Task Process()
        {
            do
            {
                var query = _url + "/contacts" + $"?limit={limit}&offset={offset}" + dateFilter;
                offset += limit;

                query += $"&status={(int)status}";
                query += querySuffix;

                System.Diagnostics.Debug.WriteLine(query);

                using var result = await DoGetAsync(query, cancellationToken);

                result.EnsureSuccessStatusCode();

                var jsonData = await result.Content.ReadAsStringAsync(cancellationToken);

                var cdr = JsonConvert.DeserializeObject<ContactDataResponse>(jsonData);

                if (cdr.contacts.Length > 0)
                {
                    o.AddRange(cdr.contacts);

                    totalProcessed += cdr.contacts.Length;

                    if (options != null) options.RaiseProcessedEvent(totalProcessed, cdr.meta.total);
                }

                if (cdr.contacts.Length < limit)
                    break;

            } while (true);
        }

        var tasks = new List<Task>();

        for (var i = 0; i < 6; ++i)
        {
            var task = Process();
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        return o.ToArray();
    }

    private async Task<HttpResponseMessage> DoGetAsync(string query, CancellationToken cancellationToken = default)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        do
        {
            try
            {
                var result = await _httpClient.GetAsync(query, cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    result.Dispose();

                    await Task.Delay(1000, cancellationToken);

                    continue;
                }

                return result;

            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {

            }
            catch (OperationCanceledException)
            {
                throw;

            }

        } while (true);
    }

    private async Task<HttpResponseMessage> DoPostAsync(string query, string content, CancellationToken cancellationToken = default)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        using var c = new StringContent(content);

        do
        {
            try
            {
                var result = await _httpClient.PostAsync(query, c, cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    result.Dispose();

                    await Task.Delay(1000, cancellationToken);

                    continue;
                }

                return result;

            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {

            }
            catch (OperationCanceledException)
            {
                throw;

            }

        } while (true);
    }

    private async Task<HttpResponseMessage> DoDeleteAsync(string query, CancellationToken cancellationToken = default)
    {
        await ActivityLimiter.WaitAsync(cancellationToken);

        do
        {
            try
            {
                var result = await _httpClient.DeleteAsync(query, cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    result.Dispose();

                    await Task.Delay(1000, cancellationToken);

                    continue;
                }

                return result;

            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {

            }
            catch (OperationCanceledException)
            {
                throw;

            }

        } while (true);
    }

    private interface IResponseListData<out T>
    {
        int Total { get; }
        T[] Elements { get; }
    }

    private struct AddOrSyncContactResponse
    {
        public ContactData contact;
    }

    private struct ContactDataResponse
    {
        public struct Meta
        {
            public int total;
        }

        public string[] scoreValues;
        public ContactData[] contacts;
        public Meta meta;
    }

    private struct TagsDataResponse : IResponseListData<TagData>
    {
        int IResponseListData<TagData>.Total => meta.total;
        TagData[] IResponseListData<TagData>.Elements => tags;

        public struct Meta
        {
            public int total;
        }

        public TagData[] tags;
        public Meta meta;

    }

    private struct ListsDataResponse : IResponseListData<ListData>
    {
        int IResponseListData<ListData>.Total => meta.total;
        ListData[] IResponseListData<ListData>.Elements => lists;

        public struct Meta
        {
            public int total;
        }

        public ListData[] lists;
        public Meta meta;
    }

    private struct ContactListStatusResponse
    {
        public ContactListStatus[] contactLists;
    }

    private struct GetContactTagsDataResponse
    {
        public struct TagAssociationData
        {
            public int contact;
            public int tag;
            public int id;
        }

        public TagAssociationData[] contactTags;
    }

    private struct GetCampaignsDataResponse : IResponseListData<CampaignData>
    {
        int IResponseListData<CampaignData>.Total => meta.total;
        CampaignData[] IResponseListData<CampaignData>.Elements => campaigns;

        public struct Meta
        {
            public int total;
        }

        public CampaignData[] campaigns;
        public Meta meta;
    }

    private struct UpdateContactResponse
    {
        public ContactListStatus contactList;

    }

    private struct GetCustomFieldsResponse : IResponseListData<CustomFieldData>
    {
        int IResponseListData<CustomFieldData>.Total => meta.total;
        CustomFieldData[] IResponseListData<CustomFieldData>.Elements => fields;

        public struct Meta
        {
            public int total;
        }

        public CustomFieldData[] fields;
        public Meta meta;
    }

    private struct GetAllContactsCustomFieldsResponse : IResponseListData<ContactCustomFieldData>
    {
        int IResponseListData<ContactCustomFieldData>.Total => meta.total;

        ContactCustomFieldData[] IResponseListData<ContactCustomFieldData>.Elements => fieldValues;

        public struct Meta
        {
            public int total;
        }

        public ContactCustomFieldData[] fieldValues;
        public Meta meta;
    }

    private struct GetContactCustomFieldsResponse
    {
        public ContactCustomFieldData[] fieldValues;
    }

    private struct GetContactTagsResponse
    {
        public struct Contact
        {
            public int contact;
            public int tag;
            public DateTime cdate;
            public DateTime created_timestamp;
            public DateTime updated_timestamp;
        }

        public Contact[] contactTags;
    }

    public string Key
    {
        set
        {
            _httpClient.DefaultRequestHeaders.Remove("Api-Token");
            _httpClient.DefaultRequestHeaders.Add("Api-Token", value);
        }
    }
    */
}
