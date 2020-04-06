using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class FilehashObject
    {
        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }
}
