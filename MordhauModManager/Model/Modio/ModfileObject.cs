using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class ModfileObject
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mod_id")]
        public int ModId { get; set; }

        [JsonProperty("date_added")]
        public int DateAdded { get; set; }


        [JsonProperty("filesize")]
        public int FileSize { get; set; }

        [JsonProperty("filehash")]
        public FilehashObject FilehashObject { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("download")]
        public DownloadObject DownloadObject { get; set; }
    }
}
