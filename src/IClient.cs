namespace ActiveCampaign;

public interface IClient
{
    public Task<T?> Get<T>(string endpoint, CancellationToken cancellationToken);

    public Task<T?> Post<T>(string endpoint, string jsonData, CancellationToken cancellationToken);

    public Task<T?> Put<T>(string endpoint, string jsonData, CancellationToken cancellationToken);
    
    public Task<bool> Delete(string endpoint, CancellationToken cancellationToken);
}