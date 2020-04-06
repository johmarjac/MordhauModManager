using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class ErrorObject
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
