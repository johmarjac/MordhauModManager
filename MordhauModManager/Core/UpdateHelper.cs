using MordhauModManager.Model.GitHub;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MordhauModManager.Core
{
    public class UpdateHelper
    {
        public static async Task<ReleaseObject> GetLatestGitHubRelease()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("User-Agent", "MordhauModManager");

                    var response = await client.GetAsync("https://api.github.com/repos/johmarjac/MordhauModManager/releases/latest");
                    var responseData = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<ReleaseObject>(responseData);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Instance.WriteLine($"Exception {ex}");
                return null;
            }
        }
    }
}
