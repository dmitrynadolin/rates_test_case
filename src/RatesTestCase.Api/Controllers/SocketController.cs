using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using RatesTestCase.Api.Services;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading;

namespace RatesTestCase.Api.Controllers;

public class SocketController : ControllerBase
{
    private readonly WebSocketSessionManager _sessionManager;
    private readonly ILogger<SocketController> _logger;

    public SocketController(WebSocketSessionManager sessionManager, ILogger<SocketController> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }


    [Route("/ws")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Stream()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        await _sessionManager.SubscribeAsync(webSocket);

        await webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
            webSocket.CloseStatusDescription,
            CancellationToken.None);
    }

    //private static long _location = 0;

    //[Route("/ws-baseline")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    //public async Task StreamTest()
    //{
    //    if (!HttpContext.WebSockets.IsWebSocketRequest)
    //    {
    //        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    //        return;
    //    }

    //    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

    //    Interlocked.Increment(ref _location);

    //    _logger.LogInformation("Test Sockets {NumberOfSessions}", _location);

    //    while (webSocket.CloseStatus == null)
    //    {
    //        var delay = Task.Delay(100);

    //        var message = new Message("Test", 1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).ToBytes();
            
    //        await webSocket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);

    //        await delay;
    //    }

    //    Interlocked.Decrement(ref _location);
    //    _logger.LogInformation("Test Sockets {NumberOfSessions}", _location);

    //    await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription,
    //        CancellationToken.None);
    //}
}
