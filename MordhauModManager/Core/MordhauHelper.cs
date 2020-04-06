using System.Diagnostics;
using System.IO;

namespace MordhauModManager.Core
{
    public class MordhauHelper
    {

        public const int MORDHAU_STEAM_APP_ID = 629760;

        public const int MORDHAU_MODIO_GAME_ID = 169;

        public static string MordhauAppManifestFile = string.Empty;

        public static string MordhauInstallationFolder = string.Empty;

        public static string GetModioPath()
        {
            if (MordhauInstallationFolder == string.Empty)
                return null;

            return Path.Combine(MordhauInstallationFolder, "Mordhau", "Content", ".modio");
        }

        public static bool IsMordhauRunning()
        {
            return Process.GetProcessesByName("Mordhau-Win64-Shipping").Length > 0;
        }

        public static void LocateMordhauAppManifestFile()
        {
            foreach (var baseInstallDir in SteamHelper.GetSteamBaseInstallFolders())
            {
                var appManifestFile = SteamHelper.GetSteamAppManifestFile(baseInstallDir, MORDHAU_STEAM_APP_ID);

                if (appManifestFile == null)
                    continue;

                MordhauAppManifestFile = appManifestFile;
                break;
            }
        }

        public static bool IsValidMordhauDirectory()
        {
            return
                Directory.Exists(MordhauInstallationFolder) &&
                Directory.Exists(Path.Combine(MordhauInstallationFolder, "Mordhau")) &&
                Directory.Exists(Path.Combine(MordhauInstallationFolder, "Engine"));
        }
    }
}
