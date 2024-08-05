using System.Net.WebSockets;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RatesTestCase.Api.Services;

public partial class FeedListenerService : BackgroundService
{
    private readonly ILogger<FeedListenerService> _logger;
    private readonly IEnumerable<ICurrencyRatesWriter> _clients;
    private readonly IHost _host;
    private readonly IOptions<BinanceFeedSettings> _options;
    private static JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    });

    public FeedListenerService(ILogger<FeedListenerService> logger, IEnumerable<ICurrencyRatesWriter> clients, IHost host,
        IOptions<BinanceFeedSettings> options)
    {
        _logger = logger;
        _clients = clients;
        _host = host;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var socket = new ClientWebSocket();
        try
        {
            // TODO: Add retry strategy
            await socket.ConnectAsync(new Uri(_options.Value.FeedUrl), stoppingToken);

            await SubscribeToAggTrades(socket, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Could not connect to the Binance feed.");
            await _host.StopAsync(stoppingToken);
        }

        while (socket.CloseStatus == null)
        {
            var buffer = new byte[1024];

            var receiveResult = await socket.ReceiveAsync(buffer, stoppingToken);

            using var ms = new MemoryStream(buffer, 0, receiveResult.Count);

            using var reader = new StreamReader(ms);
            
            await using var json = new JsonTextReader(reader);
            
            var reply = _serializer.Deserialize<AggTradeReply>(json);

            if (reply != null)
            {
                foreach (var client in _clients)
                {
                    await client.AcknowledgeAsync(reply.Market, reply.Price, reply.Time, stoppingToken);
                }
            }
        }
    }

    private async Task SubscribeToAggTrades(ClientWebSocket socket, CancellationToken stoppingToken)
    {
        var subsribeMessage = new SubscribeRequest();

        subsribeMessage.Params.AddRange(_options.Value.Pairs.Select(x => $"{x}@aggTrade"));

        using var writerStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(writerStream);

        streamWriter.AutoFlush = true;

        _serializer.Serialize(streamWriter, subsribeMessage);

        await socket.SendAsync(writerStream.ToArray(), WebSocketMessageType.Text, true, stoppingToken);


        var buffer = new byte[1024];
        var receiveResult = await socket.ReceiveAsync(buffer, stoppingToken);

        using var readerStream = new MemoryStream(buffer, 0, receiveResult.Count);
        using var reader = new StreamReader(readerStream);
        await using var json = new JsonTextReader(reader);

        var reply = _serializer.Deserialize<SubscribeReply>(json);

        if (reply == null || reply.Error != null)
        {
            // TODO: Add more specific exception type
            throw new ApplicationException($"{reply?.Error?.Code ?? -1}:{reply?.Error?.Msg ?? "<empty>"}");
        }
    }
}


