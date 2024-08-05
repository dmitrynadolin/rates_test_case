using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace RatesTestCase.Api.Services;

public class WebSocketSessionManager : ICurrencyRatesWriter, IWebSocketSessionManager
{
    private readonly ILogger<WebSocketSessionManager> _logger;
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<byte[]>> _sessions = [];

    public WebSocketSessionManager(ILogger<WebSocketSessionManager> logger)
    {
        _logger = logger;
    }

    public Task AcknowledgeAsync(string pair, decimal price, long time, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();

        var sessions = _sessions.ToArray();

        var message = new Message(pair, price, time).ToBytes();

        foreach (var session in sessions)
        {
            session.Value.Enqueue(message);
        }

        var queueLengths = sessions.Select(x => x.Value.Count).DefaultIfEmpty(0).ToArray();
        _logger.LogDebug("Binance message handled. Active queues={NumberOfClients}, Avg Size={AvgQueueSize}, Max Size={MaxQueueSize}, Time={Time}ms", sessions.Length,
            queueLengths.Average(), queueLengths.Max(), stopwatch.ElapsedMilliseconds);

        return Task.CompletedTask;
    }

    public async Task SubscribeAsync(WebSocket socket, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid();
        _sessions[sessionId] = new ConcurrentQueue<byte[]>();


        // We never read anything from the socket
        // Due to the behavior of the .NET implementation
        // if the client disconnects using normal close command
        // the conection would live until the next ping request
        while (socket.CloseStatus == null)
        {
            var delay = Task.Delay(10, cancellationToken);

            if (!_sessions.TryGetValue(sessionId, out var queue))
            {
                break;
            }

            while (queue.TryDequeue(out var message))
            {
                await socket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
            }

            await delay;
        }

        _sessions.TryRemove(sessionId, out _);
    }
}

public record Message(string Pair, decimal Price, long Time)
{
    public byte[] ToBytes() =>
        Encoding.UTF8.GetBytes(FormattableString.Invariant(
            $"{{\"m\":\"{Pair}\",\"p\":{Price},\"t\":{Time}}}"));
}
