using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using RatesTestCase.Api.Services;

namespace RatesTestCase.Api.Controllers;

public class SocketController : ControllerBase
{
    private readonly WebSocketSessionManager _sessionManager;

    public SocketController(WebSocketSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    [Route("/ws")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Task.WaitAll(_sessionManager.SubscribeAsync(webSocket));
    }
}
