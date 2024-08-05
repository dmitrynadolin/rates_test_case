using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RatesTestCase.Api.Model;
using RatesTestCase.Api.Services;

namespace RatesTestCase.Api.Controllers;

[Route("api/rates")]
[ApiController]
public class RatesController : ControllerBase
{
    private readonly ICurrencyRatesReader _ratesService;

    public RatesController(ICurrencyRatesReader ratesService)
    {
        _ratesService = ratesService;
    }

    [HttpGet]
    [ProducesResponseType<MarketsResponse>(200)]
    public async Task<IActionResult> Pairs([FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {
        return Ok(new MarketsResponse
        {
            ItemsCount = await _ratesService.GetItemsCountAsync(),
            Items = (await _ratesService.GetCurrentRatesAsync(offset, limit)).Select(r => r.Key).ToList()
        });
    }

    [HttpGet("{pair}")]
    [ProducesResponseType<PriceResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Price([FromRoute]string pair)
    {
        var response = await _ratesService.GetRateAsync(pair);

        if(response.Price == null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}
