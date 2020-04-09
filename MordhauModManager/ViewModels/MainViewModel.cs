using MordhauModManager.Core;
using MordhauModManager.Model;
using MordhauModManager.Model.Modio;
using MordhauModManager.Model.Modio.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using WPFCore;
using WPFCore.Commands;
using MessageBox = System.Windows.MessageBox;

namespace MordhauModManager.ViewModels
{
    public class MainViewModel : ViewModelBase, IViewListener
    {


        private string appStatus;
        public string AppStatus
        {
            get => appStatus;
            set
            {
                appStatus = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AppStatus)));
            }
        }

        public ICommand MordhauFolderSelectCommand { get; }

        public ICommand ReloadModsCommand { get; }

        public ICommand InfoCommand { get; }

        public ICommand InstallRemoveModCommand { get; }

        public ICommand UpdateModCommand { get; }

        public ICommand UninstallModCommand { get; }

        public ICommand DonatePayPalCommand { get; }

        public ICommand DonatePatreonCommand { get; }

        public ICommand ExitCommand { get; }

        public bool IsValidMordhauFolder
        {
            get => MordhauHelper.IsValidMordhauDirectory();
        }

        private bool isReadyForInput;
        public bool IsReadyForInput
        {
            get => isReadyForInput;
            set
            {
                isReadyForInput = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsReadyForInput)));
            }
        }

        private string filterText = string.Empty;
        public string FilterText
        {
            get => filterText;
            set
            {
                filterText = value;
                AvailableModView.Refresh();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FilterText)));
            }
        }

        private EFilterMethod filterMethod;
        public EFilterMethod FilterMethod
        {
            get => filterMethod;
            set
            {
                filterMethod = value;
                AvailableModView.Refresh();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FilterMethod)));
            }
        }

        private ICollectionView AvailableModView { get; }
        public ObservableCollection<ModObject> AvailableMods { get; }

        private ModObject selectedAvailableMod;
        public ModObject SelectedAvailableMod
        {
            get => selectedAvailableMod;
            set
            {
                selectedAvailableMod = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedAvailableMod)));
            }
        }

        public IEnumerable<EFilterMethod> FilterMethods
        {
            get
            {
                return Enum.GetValues(typeof(EFilterMethod))
                    .Cast<EFilterMethod>();
            }
        }

        public MainViewModel()
        {
            ReloadModsCommand = new RelayCommand(ReloadMods_Click);
            InfoCommand = new RelayCommand(Info_Click);
            MordhauFolderSelectCommand = new RelayCommand(ChooseMordhauFolderDialog);
            ExitCommand = new RelayCommand(() => { System.Windows.Application.Current.Shutdown(); });
            
            AvailableMods = new ObservableCollection<ModObject>();
            AvailableModView = CollectionViewSource.GetDefaultView(AvailableMods);
            AvailableModView.Filter = GetAvailableModViewFilter;

            InstallRemoveModCommand = new RelayCommand(InstallRemoveMod_Click);
            UpdateModCommand = new RelayCommand(UpdateMod_Click);

            DonatePayPalCommand = new RelayCommand(DonatePayPal_Click);
            DonatePatreonCommand = new RelayCommand(DonatePatreon_Click);
        }

        private void Info_Click()
        {
            MessageBox.Show("Mordhau Mod Manager by johmarjac\n\nIf you find bugs let me know. You can contact me in Discord: johmarjac#3365\n\nHave Fun!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ReloadMods_Click()
        {
            AvailableMods.Clear();

            AppStatus = "Reading mod.io api...";
            ModioHelper.LoadModioAccessToken(MordhauHelper.GetModioPath());
            await LoadMySubscriptions();
            await LoadAvailableMods();
            await CheckIfModsAreUp2Date();

            IsReadyForInput = true;

            AppStatus = "Ready";
        }

        private async void UpdateMod_Click()
        {
            if (SelectedAvailableMod == null)
                return;

            var modReference = SelectedAvailableMod;

            await RemoveModLogic(modReference);
            await InstallModLogic(modReference);

            FilterMethod = EFilterMethod.InstalledOnly;
        }

        private void DonatePatreon_Click()
        {
            try
            {
                using (var p = new Process())
                {
                    p.StartInfo.FileName = "https://patreon.com/johmarjac";
                    p.StartInfo.Verb = "open";
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                }
            }
            catch (Exception) { }
        }

        private void DonatePayPal_Click()
        {
            try
            {
                using (var p = new Process())
                {
                    p.StartInfo.FileName = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=YHRLW7F5SRAH8&source=url";
                    p.StartInfo.Verb = "open";
                    p.StartInfo.UseShellExecute = true;
                    p.Start();
                }
            }
            catch (Exception) { }
        }

        private async void InstallRemoveMod_Click()
        {
            if (SelectedAvailableMod == null)
                return;

            if(!SelectedAvailableMod.IsInstalled)
            {
                await InstallModLogic(SelectedAvailableMod);
            }
            else
            {
                await RemoveModLogic(SelectedAvailableMod);
            }
        }

        private async Task InstallModLogic(ModObject modToInstall)
        {
            AppStatus = $"Subscribing to {modToInstall.Name}...";
            modToInstall.IsInstalling = true;

            // Subscribe to mod then download and install
            var modObject = await ModioHelper.SubscribeToModObject(modToInstall);
            if (modObject != null)
            {
                AppStatus = $"Downloading and installing {modToInstall.Name}...";

                // Install mod to mordhau
                var result = await ModioHelper.DownloadAndInstallMod(modToInstall, MordhauHelper.GetModioPath());
                
                if(result)
                {
                    modToInstall.IsInstalled = true;
                    modToInstall.IsInstalling = false;
                    modToInstall.IsUpdateAvailable = false;

                    AvailableModView.Refresh();
                    AppStatus = $"Ready";
                }
                else
                {
                    AppStatus = $"Mod Installation failed!";
                    MessageBox.Show("Mod installation failed, please contact developer!", "Installation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                AppStatus = $"Subscribing failed!";
                MessageBox.Show("Subscribing to the mod failed, please contact developer!", "Subscription Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RemoveModLogic(ModObject modToRemove)
        {
            AppStatus = $"Unsubscribing from {modToRemove.Name}...";
            modToRemove.IsInstalling = true;

            // Unsubscribe from mod then remove local files
            var result = await ModioHelper.UnsubscribeFromModObject(modToRemove);
            if (result)
            {
                AppStatus = $"Removing {modToRemove.Name} from disk...";

                // Remove mod from mordhau
                var result2 = await ModioHelper.RemoveModFromDisk(modToRemove, MordhauHelper.GetModioPath());

                if(result2)
                {
                    modToRemove.IsInstalling = false;
                    modToRemove.IsInstalled = false;
                    modToRemove.IsUpdateAvailable = false;
                    AvailableModView.Refresh();

                    AppStatus = $"Ready";
                }
                else
                {
                    AppStatus = $"Mod Removal failed!";
                    MessageBox.Show("Uninstalling mod failed, please contact developer!", "Uninstallation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                AppStatus = $"Unsubscribing failed!";
                MessageBox.Show("Unsubscribing from the mod failed, please contact developer!", "Unsubscription Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool GetAvailableModViewFilter(object obj)
        {
            if (obj is ModObject mo)
            {
                var DoesNameContainsFilterText = mo.Name != null && mo.Name.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase);
                var CheckInstalled = FilterMethod == EFilterMethod.InstalledOnly && mo.IsInstalled;
                var CheckUpdates = FilterMethod == EFilterMethod.UpdateAvailable && mo.IsUpdateAvailable && mo.IsInstalled;
                var CheckRest = FilterMethod == EFilterMethod.All;

                return DoesNameContainsFilterText && (CheckInstalled || CheckUpdates || CheckRest);
            }
            else
                return false;
        }

        public async void OnLoaded()
        {
            await CheckUpdate();

            if(MordhauHelper.IsMordhauRunning())
            {
                MessageBox.Show("Mordhau is currently running, please close Mordhau first to reliably install and remove mods!", "Close Mordhau", MessageBoxButton.OK, MessageBoxImage.Warning);
                System.Windows.Application.Current.Shutdown();
                return;
            }

            AppStatus = "Looking for Steam...";

            if(!SteamHelper.IsSteamInstalled())
            {
                MessageBox.Show("Unable to find Steam installation, please locate the Mordhau Folder yourself in the following dialog.", "Steam not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                ChooseMordhauFolderDialog();
                return;
            }

            MordhauHelper.LocateMordhauAppManifestFile();
            if (MordhauHelper.MordhauAppManifestFile == string.Empty)
            {
                MessageBox.Show("Unable to find Mordhau App Manifest, please contact developer.", "Manifest not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AppStatus = "Looking for Mordhau App Manifest...";
            MordhauHelper.LocateMordhauAppManifestFile();

            MordhauHelper.MordhauInstallationFolder = SteamHelper.GetInstallDirFromAppManifest(MordhauHelper.MordhauAppManifestFile);
            if(!IsValidMordhauFolder)
            {
                MessageBox.Show("Unable to find Mordhau installation, please locate the Mordhau Folder yourself in the following dialog.", "Steam not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                ChooseMordhauFolderDialog();
                return;
            }

            AppStatus = "Reading mod.io api...";
            ModioHelper.LoadModioAccessToken(MordhauHelper.GetModioPath());
            if(ModioHelper.AccessToken == null)
            {
                MessageBox.Show("Unable to read authentication.json, make sure you ran MORDHAU at least once and played on a modded server at least once before using this tool.", "Authentication.json not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                System.Windows.Application.Current.Shutdown();
                return;
            }

            await LoadMySubscriptions();
            await LoadAvailableMods();
            await CheckIfModsAreUp2Date();

            IsReadyForInput = true;

            AppStatus = "Ready";
        }

        private async Task CheckUpdate()
        {
            var latestRelease = await UpdateHelper.GetLatestGitHubRelease();

            if (latestRelease == null)
                return;

            if(latestRelease.tag_name != App.APP_VERSION && latestRelease.assets.Count >= 1)
            {
                if(MessageBox.Show($"A new update ({latestRelease.tag_name})  was found. Do you want to install it now?", $"{latestRelease.tag_name} is here!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var p = new Process())
                        {
                            p.StartInfo.FileName = "https://github.com/johmarjac/MordhauModManager/releases/latest";
                            p.StartInfo.Verb = "open";
                            p.StartInfo.UseShellExecute = true;
                            p.Start();
                        }

                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception) { }
                }
            }
        }

        private async Task CheckIfModsAreUp2Date()
        {
            bool foundUpdates = false;

            foreach(var mod in AvailableMods.Where(x => x.IsInstalled))
            {
                var localModObject = ModioHelper.GetLocalModObject(mod.Id);
                if(localModObject == null)
                {
                    if(MessageBox.Show($"Something is not correct with the mod '{mod.Name}', do you want to reinstall it? If you select no, I will unsubscribe you from the mod and clean up the corrupted files on your disk.", "Corrupt", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        var res1 = await ModioHelper.RemoveModFromDisk(mod, MordhauHelper.GetModioPath());
                        var res2 = await ModioHelper.DownloadAndInstallMod(mod, MordhauHelper.GetModioPath());

                        if (res1 && res2)
                        {
                            MessageBox.Show("I have successfully re-installed the mod and updated it to the latest version!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        var res1 = await ModioHelper.UnsubscribeFromModObject(mod);
                        var res2 = await ModioHelper.RemoveModFromDisk(mod, MordhauHelper.GetModioPath());

                        if(res1 && res2)
                        {
                            mod.IsInstalling = false;
                            mod.IsInstalled = false;
                            AvailableModView.Refresh();

                            MessageBox.Show("I have successfully unsubscribed you from the mod and removed the corrupted mod files from your disk!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    if(mod.ModFileObject.Version != localModObject.ModFileObject.Version)
                    {
                        mod.IsUpdateAvailable = true;
                        mod.InstallStatusImage = ModObject.UpdateAvailableIcon;

                        foundUpdates = true;
                    }
                }
            }

            if(foundUpdates)
                FilterMethod = EFilterMethod.UpdateAvailable;

            AvailableModView.Refresh();
        }

        private async Task LoadMySubscriptions()
        {
            int offset = 0;

            GetModsResponse response = null;

            while (true)
            {
                response = await ModioHelper.GetUserSubscriptions(MordhauHelper.MORDHAU_MODIO_GAME_ID, ModioHelper.AccessToken, FilterText, offset);
                offset += response.ResultsAmount;

                foreach (var mod in response.ModObjects)
                {
                    mod.IsInstalled = true;
                    AvailableMods.Add(mod);
                }

                if (offset >= response.TotalResults)
                    break;
            }
        }

        private async Task LoadAvailableMods()
        {
            int offset = 0;

            GetModsResponse response = null;

            while(true)
            {
                response = await ModioHelper.GetModsAsync(FilterText, offset);
                offset += response.ResultsAmount;

                foreach (var mod in response.ModObjects)
                {
                    if (!AvailableMods.Any(x => x.Id == mod.Id))
                        AvailableMods.Add(mod);
                }

                if (offset >= response.TotalResults)
                    break;
            }
        }

        private async void ChooseMordhauFolderDialog()
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                MordhauHelper.MordhauInstallationFolder = fbd.SelectedPath;

                if (!MordhauHelper.IsValidMordhauDirectory())
                {
                    if (MessageBox.Show("The selected folder seems to be invalid, do you want to try locating it again?", "Wrong folder", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        ChooseMordhauFolderDialog();
                    }
                }
                else
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsValidMordhauFolder)));

                    AppStatus = "Reading mod.io api...";
                    ModioHelper.LoadModioAccessToken(MordhauHelper.GetModioPath());
                    await LoadMySubscriptions();
                    await LoadAvailableMods();
                    await CheckIfModsAreUp2Date();

                    IsReadyForInput = true;

                    AppStatus = "Ready";
                }
            }
            else
                AppStatus = "Mordhau Installation Folder invalid!";
        }

        public void OnClosing()
        {

        }
    }
}
