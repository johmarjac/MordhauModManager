using MordhauModManager.Model.Modio.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MordhauModManager.Core
{
    public class ModioHelper
    {

        private const string ApiUrl = "https://api.mod.io/v1";

        public static string AccessToken { get; set; }

        public static async Task<GetModsResponse> GetUserSubscriptions(int gameId, string accessToken, string filter = "", int offset = 0)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var responseData = await client.GetStringAsync(ApiUrl + $"/me/subscribed?game_id={gameId}&_q={filter}&_offset={offset}");

                return JsonConvert.DeserializeObject<GetModsResponse>(responseData);
            }
        }

        public static async Task<GetModsResponse> GetModsAsync(int gameId, string accessToken, string filter = "", int offset = 0)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var responseData = await client.GetStringAsync(ApiUrl + $"/games/{gameId}/mods?_offset={offset}&_q={filter}");

                return JsonConvert.DeserializeObject<GetModsResponse>(responseData);
            }
        }

        public static void LoadModioAccessToken(string modioFolder)
        {
            if (modioFolder == null)
                return;

            var authorizationFile = Path.Combine(modioFolder, "authentication.json");
            if (!File.Exists(authorizationFile))
                return;

            try
            {
                var authorizationData = JObject.Parse(File.ReadAllText(authorizationFile));
                AccessToken = authorizationData["access_token"].ToObject<string>();
            }
            catch(JsonReaderException)
            {
            }
        }
    }
}
