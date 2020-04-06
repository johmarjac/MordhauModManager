using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio.Responses
{
    public abstract class ResponseBase
    {
        [JsonProperty("error")]
        public ErrorObject Error { get; set; }
    }
}
