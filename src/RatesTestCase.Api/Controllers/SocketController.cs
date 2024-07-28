using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace RatesTestCase.Api.Controllers;

public class SocketController : ControllerBase
{
    [Route("/stream")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
