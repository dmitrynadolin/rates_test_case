using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RatesTestCase.Api.Services;

public partial class FeedListenerService
{
    public class AggTradeReply
    {
        [JsonProperty("s")] public string Market { get; set; }
        [JsonProperty("p")] public decimal Price { get; set; }
        [JsonProperty("T")] public long Time { get; set; }
    }
}


