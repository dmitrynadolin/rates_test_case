using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace RatesTestCase.Api.Services;

public class WebSocketSessionManager
{
    private ConcurrentDictionary<Guid, WebSocket> _sessions = [];
    
    public void AcknowledgeAsync(string market, decimal price, CancellationToken cancellationToken = default)
    {
        var sessions = _sessions.ToArray();

        var message = Encoding.UTF8.GetBytes(FormattableString.Invariant($"{{\"m\":\"{market}\",\"p\":{price}}}"));

        foreach (var socket in sessions)
        {
            if (socket.Value.CloseStatus != null)
            {
                _sessions.TryRemove(socket.Key, out _);

                continue;
            }

            socket.Value.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    public async Task SubscribeAsync(WebSocket socket, CancellationToken cancellationToken = default)
    {
        _sessions[Guid.NewGuid()] = socket;

        while (socket.CloseStatus == null)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
