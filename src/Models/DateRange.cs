namespace ActiveCampaign.Models;

public struct DateRange
{
    public DateRange(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException("The start date cannot be set after end date", "start");

        Start = start;
        End = end;
    }

    public DateTime Start { get; }
    public DateTime End { get; }
}
