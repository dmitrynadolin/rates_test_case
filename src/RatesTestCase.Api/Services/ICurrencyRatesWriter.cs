namespace RatesTestCase.Api.Services;
public interface ICurrencyRatesWriter
{
    Task AcknowledgeAsync(string pair, decimal price, long time, CancellationToken cancellationToken = default);
}
