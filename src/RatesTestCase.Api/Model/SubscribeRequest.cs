namespace RatesTestCase.Api.Services;

public partial class FeedListenerService
{
    public class SubscribeRequest
    {
        public string Method => "SUBSCRIBE";
        public List<string> Params { get; } = [];
        public ulong Id { get; set; }
    }
}


