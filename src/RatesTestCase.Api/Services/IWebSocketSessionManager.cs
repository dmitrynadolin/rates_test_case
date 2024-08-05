using System.Net.WebSockets;

namespace RatesTestCase.Api.Services;

public interface IWebSocketSessionManager
{
    Task SubscribeAsync(WebSocket socket, CancellationToken cancellationToken = default);
}
