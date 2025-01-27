namespace ActiveCampaign;

public class GetCampaignsOptions
{
    public event System.Action<int, int> OnProcessed = delegate { };

    internal void RaiseProcessedEvent(int processed, int total)
    {
        OnProcessed(processed, total);
    }
}

