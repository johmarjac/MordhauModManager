using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class ModDependencyObject
    {
        [JsonProperty("mod_id")]
        public int ModId { get; set; }

        [JsonProperty("date_added")]
        public int DateAdded { get; set; }
    }
}
