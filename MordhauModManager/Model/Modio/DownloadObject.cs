using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class DownloadObject
    {
        [JsonProperty("binary_url")]
        public string BinaryUrl { get; set; }

        [JsonProperty("date_expires")]
        public int DateExpires { get; set; }
    }
}
