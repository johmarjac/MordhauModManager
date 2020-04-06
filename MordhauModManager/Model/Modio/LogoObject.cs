using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class LogoObject
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("thumb_320x180")]
        public string Thumb_320x180 { get; set; }

        [JsonProperty("thumb_640x360")]
        public string Thumb_640x360 { get; set; }

        [JsonProperty("thumb_1280x720")]
        public string Thumb_1280x720 { get; set; }
    }
}
