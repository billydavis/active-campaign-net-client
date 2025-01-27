namespace ActiveCampaign;

public class ResourceGuard : IDisposable
{
    private int MaxConcurrentAccesses => _lastQueriesTimeStamps.Length;

    private readonly int _accessLifeTimeMilliseconds;
    private readonly SemaphoreSlim _semaphore;
    private readonly DateTime[] _lastQueriesTimeStamps;
    private int _lastQueriesTimeStampsCount;


    public ResourceGuard(int maxConcurrentAccesses, int accessLifeTimeMilliseconds)
    {
        _semaphore = new SemaphoreSlim(1);
        _accessLifeTimeMilliseconds = accessLifeTimeMilliseconds;
        _lastQueriesTimeStamps = new DateTime[maxConcurrentAccesses];
    }

    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            if (_lastQueriesTimeStampsCount > 0 && _lastQueriesTimeStampsCount == MaxConcurrentAccesses)
            {
                var oldestTimeStamp = _lastQueriesTimeStamps[0];
                var diff = DateTime.Now - oldestTimeStamp;
                var milliseconds = diff.Ticks / TimeSpan.TicksPerMillisecond;

                if (milliseconds < _accessLifeTimeMilliseconds)
                {
                    var remaining = _accessLifeTimeMilliseconds - (int)milliseconds;

                    await Task.Delay(remaining, cancellationToken);
                }

                RemoveOldestTimeStamp();
            }

            AddTimeStamp();

        }
        finally
        {
            _semaphore.Release();

        }
    }

    private void AddTimeStamp()
    {
        _lastQueriesTimeStamps[_lastQueriesTimeStampsCount] = DateTime.Now;
        ++_lastQueriesTimeStampsCount;
    }

    private void RemoveOldestTimeStamp()
    {
        Array.Copy(_lastQueriesTimeStamps, 1, _lastQueriesTimeStamps, 0, _lastQueriesTimeStamps.Length - 1);
        --_lastQueriesTimeStampsCount;
    }

    void IDisposable.Dispose()
    {
        _semaphore.Dispose();
    }

    
}
