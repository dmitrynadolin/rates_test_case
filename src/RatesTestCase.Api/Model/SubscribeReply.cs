namespace RatesTestCase.Api.Services;

public partial class FeedListenerService
{
    public class SubscribeReply
    {
        public ulong? Id { get; set; }
        public BinanceApiError? Error { get; set; }
    }
}


