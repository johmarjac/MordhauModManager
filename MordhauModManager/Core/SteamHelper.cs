using Gameloop.Vdf;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MordhauModManager.Core
{
    public class SteamHelper
    {

        public static RegistryKey GetSteamRegistryKey()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        }

        public static bool IsSteamInstalled()
        {
            return GetSteamRegistryKey() != null;
        }

        public static string GetSteamPath()
        {
            if (!IsSteamInstalled())
                return null;

            var regKey = GetSteamRegistryKey();

            if (regKey == null)
                return null;

            return (string)regKey.GetValue("SteamPath", null);
        }

        public static string GetSteamConfigDirectory()
        {
            var steamPath = GetSteamPath();

            if (steamPath == null || !Directory.Exists(steamPath))
                return null;

            return Path.Combine(steamPath, "config");
        }

        public static string GetSteamConfigFilePath()
        {
            var configDir = GetSteamConfigDirectory();

            if (configDir == null || !Directory.Exists(configDir))
                return null;

            return Path.Combine(configDir, "config.vdf");
        }

        public static string[] GetSteamBaseInstallFolders()
        {
            var configFilePath = GetSteamConfigFilePath();

            if (configFilePath == null || !File.Exists(configFilePath))
                return null;

            var fileContent = File.ReadAllText(configFilePath);

            var configModel = VdfConvert.Deserialize(fileContent);

            if (configModel.Key != "InstallConfigStore")
                return null;

            var baseInstallFolders = new List<string>();

            baseInstallFolders.Add(GetSteamPath());

            foreach(var softwareChild in configModel.Value.Children())
            {
                if(softwareChild.Key == "Software")
                {
                    foreach(var valveChild in softwareChild.Value.Children())
                    {
                        if(valveChild.Key == "valve")
                        {
                            foreach(var steamChild in valveChild.Value.Children())
                            {
                                if(steamChild.Key == "Steam")
                                {
                                    foreach(var baseInstallFolder in steamChild.Value.Children().Where(c => c.Key.StartsWith("BaseInstallFolder_")))
                                    {
                                        baseInstallFolders.Add(baseInstallFolder.Value.ToString());
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            return baseInstallFolders.ToArray();
        }

        public static string GetSteamAppManifestFile(string baseInstallFolder, int appId)
        {
            if (baseInstallFolder == null || !Directory.Exists(baseInstallFolder))
                return null;

            var steamappsDirectory = Path.Combine(baseInstallFolder, "steamapps");
            
            if (!Directory.Exists(steamappsDirectory))
                return null;

            var filename = Path.Combine(steamappsDirectory, $"appmanifest_{appId}.acf");

            if (!File.Exists(filename))
                return null;

            return filename;
        }

        public static string GetInstallDirFromAppManifest(string appManifestFile)
        {
            if (!File.Exists(appManifestFile))
                return null;

            var fileContent = File.ReadAllText(appManifestFile);

            var appStateModel = VdfConvert.Deserialize(fileContent);

            if (appStateModel.Key != "AppState")
                return null;

            string installDir = null;

            foreach(var appStateChild in appStateModel.Value.Children())
            {
                if(appStateChild.Key == "installdir")
                {
                    installDir = appStateChild.Value.ToString();
                    break;
                }
            }

            if (installDir == null)
                return null;

            return Path.Combine(Path.GetDirectoryName(appManifestFile), "common", installDir);
        }

    }
}
