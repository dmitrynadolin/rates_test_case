using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RatesTestCase.Api.Controllers;

[Route("api/rates")]
[ApiController]
public class RatesController : ControllerBase
{
    [HttpGet]
    public async Task<MarketsResponse> Markets(int offset = 0, int limit = 10)
    {
        return new MarketsResponse
        {
            ItemsCount = 0,
        };
    }

    [HttpGet("{market}")]
    public async Task<PriceResponse> Price(string market)
    {
        return new PriceResponse(Market: market, Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Price: 0);
    }
}

public record PriceResponse(string Market, long Timestamp, decimal Price);

public class MarketsResponse
{
    public List<string> Items { get; } = [];
    public int ItemsCount { get; set; }
}
