using Newtonsoft.Json;
using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Responses;

public class RecommendationResponse : ResponseBase<List<Recommendation>>
{
   // [JsonProperty("result")] public List<Recommendation> Recommendations { get; set; }
}
