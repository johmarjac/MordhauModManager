using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio.Responses
{
    public class GetModDependenciesResponse : ResponseBase
    {
        [JsonProperty("data")]
        public ModDependencyObject[] ModDependencies { get; set; }

        [JsonProperty("result_count")]
        public int ResultsAmount { get; set; }

        [JsonProperty("result_offset")]
        public int ResultOffset { get; set; }

        [JsonProperty("result_limit")]
        public int ResultLimit { get; set; }

        [JsonProperty("result_total")]
        public int TotalResults { get; set; }
    }
}
