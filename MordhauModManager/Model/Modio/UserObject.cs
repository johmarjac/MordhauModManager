using Newtonsoft.Json;

namespace MordhauModManager.Model.Modio
{
    public class UserObject
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("profile_url")]
        public string ProfileUrl { get; set; }
    }
}
