using RatesTestCase.Api.Model;

namespace RatesTestCase.Api.Services;

/// <summary>
/// In real life methods for this interface are async.
/// </summary>
public interface ICurrencyRatesReader
{
    Task<Dictionary<string,decimal>> GetCurrentRatesAsync(int offset, int limit, CancellationToken cancellationToken = default);
    Task<int> GetItemsCountAsync(CancellationToken cancellationToken = default);
    Task<PriceResponse> GetRateAsync(string pair, CancellationToken cancellationToken = default);
}
