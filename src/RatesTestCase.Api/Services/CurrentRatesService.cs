using RatesTestCase.Api.Model;
using System.Collections.Concurrent;

namespace RatesTestCase.Api.Services;

/// <summary>
/// A stub for the real storage
/// </summary>
public class CurrentRatesService : ICurrencyRatesWriter, ICurrencyRatesReader
{
    // It doesn't necessary need to be thread safe because in the current scenario it is accessed only from one thread.
    private readonly ConcurrentDictionary<string, decimal> _rates = [];

    public Task AcknowledgeAsync(string pair, decimal price, long time, CancellationToken cancellationToken = default)
    {
        _rates[pair] = price;

        return Task.CompletedTask;
    }

    public Task<Dictionary<string, decimal>> GetCurrentRatesAsync(int offset, int limit, CancellationToken cancellationToken)
    {
        var result = _rates.ToArray().OrderBy(x => x.Key).Skip(offset).Take(limit).ToDictionary(x => x.Key, x => x.Value);

        return Task.FromResult(result);
    }

    public Task<int> GetItemsCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_rates.Count);
    }

    public Task<PriceResponse> GetRateAsync(string pair, CancellationToken cancellationToken = default)
    {
        if(!_rates.TryGetValue(pair, out var rate))
        {
            return Task.FromResult(new PriceResponse(pair, null));
        }

        return Task.FromResult(new PriceResponse(pair, rate));
    }
}
