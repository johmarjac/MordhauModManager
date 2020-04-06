using MordhauModManager.Model.Modio;
using MordhauModManager.Model.Modio.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
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
            catch (JsonReaderException)
            {
            }
        }

        public static async Task<ModObject> SubscribeToModObject(ModObject modObject)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

                var response = await client.PostAsync(ApiUrl + $"/games/{modObject.GameId}/mods/{modObject.Id}/subscribe", new FormUrlEncodedContent(new KeyValuePair<string, string>[0]));
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return JsonConvert.DeserializeObject<ModObject>(await response.Content.ReadAsStringAsync());
                }
                else
                    return null;
            }
        }

        public static async Task<bool> UnsubscribeFromModObject(ModObject modObject)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken}");

                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, ApiUrl + $"/games/{modObject.GameId}/mods/{modObject.Id}/subscribe");
                requestMessage.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

                var response = await client.SendAsync(requestMessage);
                return response.StatusCode == System.Net.HttpStatusCode.NoContent;
            }
        }

        public static async Task<bool> RemoveModFromDisk(ModObject modObject, string modioFolder)
        {
            var modFolder = Path.Combine(modioFolder, "mods", $"{modObject.Id}");

            if (!Directory.Exists(modFolder))
                return true;

            try
            {
                await Task.Run(() => { Directory.Delete(modFolder, true); });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> DownloadAndInstallMod(ModObject modObject, string modioFolder)
        {
            var modFolder = Path.Combine(modioFolder, "mods", $"{modObject.Id}");

            try
            {
                await Task.Run(async () =>
                {
                    if (!Directory.Exists(modFolder))
                        Directory.CreateDirectory(modFolder);

                    var localArchivePath = Path.Combine(modFolder, modObject.ModFileObject.Filename);

                    // Download
                    using (var httpClient = new HttpClient())
                    using (var fileWriter = new FileStream(localArchivePath, FileMode.Create))
                    {
                        using (var dlStream = await httpClient.GetStreamAsync(modObject.ModFileObject.DownloadObject.BinaryUrl))
                        {
                            var buffer = new byte[1024];
                            while (true)
                            {
                                var bytesRead = dlStream.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    fileWriter.Write(buffer, 0, bytesRead);
                                    modObject.InstallProgress = (int)(((float)fileWriter.Position / modObject.ModFileObject.FileSize) * 75);
                                }
                                else
                                    break;
                            }
                        }
                    }

                    // Extract
                    ZipFile.ExtractToDirectory(localArchivePath, modFolder);

                    // Remove archive
                    File.Delete(localArchivePath);

                    // Serialize mod object to disk
                    File.WriteAllText(Path.Combine(modFolder, "modio.json"), JsonConvert.SerializeObject(modObject, Formatting.Indented));
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
