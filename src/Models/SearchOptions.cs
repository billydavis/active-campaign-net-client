namespace ActiveCampaign.Models;

public class SearchOptions
{
    public event Action<decimal, decimal> OnProcessed = delegate { };

    internal void RaiseProcessedEvent(decimal processed, decimal total)
    {
        OnProcessed(processed, total);
    }
}