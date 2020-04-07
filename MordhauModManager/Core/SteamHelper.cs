using Gameloop.Vdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MordhauModManager.Core
{
    public class SteamHelper
    {

        public static RegistryKey GetSteamRegistryKey()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

                FileLogger.Instance.WriteLine($"RegistryKey `{key}` exists!");

                return key;
            }
            catch (Exception ex)
            {
                FileLogger.Instance.WriteLine($"Exception: {ex}");
                return null;
            }
        }

        public static bool IsSteamInstalled()
        {
            FileLogger.Instance.WriteLine("Call");
            return GetSteamRegistryKey() != null;
        }

        public static string GetSteamPath()
        {
            if (!IsSteamInstalled())
                return null;

            var regKey = GetSteamRegistryKey();

            if (regKey == null)
                return null;

            var retVal =  (string)regKey.GetValue("SteamPath", null);

            FileLogger.Instance.WriteLine($"SteamPath: {(retVal == null ? "null" : retVal)}");

            return retVal;
        }

        public static string GetSteamConfigDirectory()
        {
            var steamPath = GetSteamPath();

            if (steamPath == null || !Directory.Exists(steamPath))
            {
                FileLogger.Instance.WriteLine($"{(steamPath == null ? "null" : "Directory doesnt exist")}");
                return null;
            }

            var configDir = Path.Combine(steamPath, "config");

            FileLogger.Instance.WriteLine($"ConfigDir: {configDir}");

            return configDir;
        }

        public static string GetSteamConfigFilePath()
        {
            var configDir = GetSteamConfigDirectory();

            if (configDir == null || !Directory.Exists(configDir))
            {
                FileLogger.Instance.WriteLine($"{(configDir == null ? "null" : "Directory doesnt exist")}");
                return null;
            }

            var configFile = Path.Combine(configDir, "config.vdf");

            FileLogger.Instance.WriteLine($"ConfigFile: {configFile}");

            return configFile;
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
                if(softwareChild.Key.Equals("Software", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach(var valveChild in softwareChild.Value.Children())
                    {
                        if(valveChild.Key.Equals("valve", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach(var steamChild in valveChild.Value.Children())
                            {
                                if(steamChild.Key.Equals("Steam", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    foreach(var baseInstallFolder in steamChild.Value.Children().Where(c => c.Key.StartsWith("BaseInstallFolder_")))
                                    {
                                        FileLogger.Instance.WriteLine($"Found Install Folder: {baseInstallFolder.Key} with Value: {baseInstallFolder.Value}");
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
            {
                FileLogger.Instance.WriteLine($"{(baseInstallFolder == null ? "null" : "Directory doesnt exist")}");
                return null;
            }

            var steamappsDirectory = Path.Combine(baseInstallFolder, "steamapps");
            
            if (!Directory.Exists(steamappsDirectory))
            {
                FileLogger.Instance.WriteLine($"SteamApps Directory does not exist.");
                return null;
            }

            var filename = Path.Combine(steamappsDirectory, $"appmanifest_{appId}.acf");

            if (!File.Exists(filename))
            {
                FileLogger.Instance.WriteLine($"AppManifest {filename} does not exist.");
                return null;
            }

            FileLogger.Instance.WriteLine($"AppManifest {filename} exists!");

            return filename;
        }

        public static string GetInstallDirFromAppManifest(string appManifestFile)
        {
            if (!File.Exists(appManifestFile))
            {
                FileLogger.Instance.WriteLine($"AppManifest file does not exist.");
                return null;
            }

            var fileContent = File.ReadAllText(appManifestFile);

            var appStateModel = VdfConvert.Deserialize(fileContent);

            if (appStateModel.Key != "AppState")
            {
                FileLogger.Instance.WriteLine($"Key != AppState");
                return null;
            }

            string installDir = null;

            foreach(var appStateChild in appStateModel.Value.Children())
            {
                if(appStateChild.Key == "installdir")
                {
                    FileLogger.Instance.WriteLine($"Found installdir key with value: {appStateChild.Value}");
                    installDir = appStateChild.Value.ToString();
                    break;
                }
            }

            if (installDir == null)
            {
                FileLogger.Instance.WriteLine($"installDir == null");
                return null;
            }

            var finalInstallDir = Path.Combine(Path.GetDirectoryName(appManifestFile), "common", installDir);

            FileLogger.Instance.WriteLine($"FinalInstallDir {finalInstallDir}");

            return finalInstallDir;
        }

    }
}
