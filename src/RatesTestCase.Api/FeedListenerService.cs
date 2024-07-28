using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TestCaseRates.Api;

public class FeedListenerService : BackgroundService
{
    private readonly ILogger<FeedListenerService> _logger;
    private readonly IHost _host;

    private static JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    });

    public FeedListenerService(ILogger<FeedListenerService> logger, IHost host)
    {
        _logger = logger;
        _host = host;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri("wss://stream.binance.com/ws/btcusdt"), stoppingToken);

        {
            var subsribeMessage = new SubscribeRequest()
            {
                Params = { "btcusdt@aggTrade" }
            };

            using (var ms = new MemoryStream())
            await using (var streamWriter = new StreamWriter(ms))
            {
                streamWriter.AutoFlush = true;

                _serializer.Serialize(streamWriter, subsribeMessage);

                await socket.SendAsync(ms.ToArray(), WebSocketMessageType.Text, true, stoppingToken);



            }

            var buffer = new byte[1024];
            var receiveResult = await socket.ReceiveAsync(buffer, stoppingToken);

            using (var ms = new MemoryStream(buffer, 0, receiveResult.Count))
            using (var reader = new StreamReader(ms))
            await using (var json = new JsonTextReader(reader))
            {
                var reply = _serializer.Deserialize<SubscribeReply>(json);

                if (reply == null || reply.Error != null)
                {
                    _logger.LogCritical("Failed to subscribe to the feed. Error Code={ErrorCode}, Msg={ErrorMessage}",
                        reply?.Error?.Code ?? -1,
                        reply?.Error?.Msg ?? "<empty>");

                    // Without the feed there is nothing else to do, just kill the service
                    await _host.StopAsync(stoppingToken);
                }
            }
        }

        while (socket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024];

            var receiveResult = await socket.ReceiveAsync(buffer, stoppingToken);

            using (var ms = new MemoryStream(buffer, 0, receiveResult.Count))
            using (var reader = new StreamReader(ms))
            await using (var json = new JsonTextReader(reader))
            {
                var reply = _serializer.Deserialize<AggTradeReply>(json);

                _logger.LogInformation("Received market price {Market}={Price}", reply.Market, reply.Price);
            }

            return;
        }
    }

    public class SubscribeRequest
    {
        public string Method => "SUBSCRIBE";
        public List<string> Params { get; } = [];
        public ulong Id { get; set; }
    }

    public class SubscribeReply
    {
        public ulong? Id { get; set; }
        public BinanceApiError? Error { get; set; }
    }

    public record BinanceApiError(int Code, string Msg);

    public class AggTradeReply
    {
        [JsonProperty("s")] public string Market { get; set; }

        [JsonProperty("p")] public decimal Price { get; set; }
    }
}


