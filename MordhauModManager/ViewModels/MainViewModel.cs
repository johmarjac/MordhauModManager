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

        public ICommand InstallRemoveModCommand { get; }

        public ICommand UninstallModCommand { get; }

        public ICommand DonatePayPalCommand { get; }

        public ICommand DonatePatreonCommand { get; }

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

        private ESortMethod sortMethod;
        public ESortMethod SortMethod
        {
            get => sortMethod;
            set
            {
                sortMethod = value;

                AvailableModView.SortDescriptions.Clear();

                if (sortMethod == ESortMethod.DateAdded)
                    AvailableModView.SortDescriptions.Add(new SortDescription("DateAdded", ListSortDirection.Descending));
                else if (sortMethod == ESortMethod.LastUpdated)
                    AvailableModView.SortDescriptions.Add(new SortDescription("DateUpdated", ListSortDirection.Descending));

                AvailableModView.Refresh();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SortMethod)));
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

        public ObservableCollection<ModObject> InstalledMods { get; }

        public IEnumerable<ESortMethod> SortMethods
        {
            get
            {
                return Enum.GetValues(typeof(ESortMethod))
                    .Cast<ESortMethod>();
            }
        }

        public MainViewModel()
        {
            MordhauFolderSelectCommand = new RelayCommand(ChooseMordhauFolderDialog);
            AvailableMods = new ObservableCollection<ModObject>();
            AvailableModView = CollectionViewSource.GetDefaultView(AvailableMods);
            AvailableModView.Filter = GetAvailableModViewFilter;
            AvailableModView.SortDescriptions.Add(new SortDescription("DateAdded", ListSortDirection.Descending));
            InstalledMods = new ObservableCollection<ModObject>();
            InstallRemoveModCommand = new RelayCommand(InstallRemoveMod_Click);
            UninstallModCommand = new RelayCommand(UninstallMod_Click);
            DonatePayPalCommand = new RelayCommand(DonatePayPal_Click);
            DonatePatreonCommand = new RelayCommand(DonatePatreon_Click);
        }

        private void UninstallMod_Click()
        {

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

        private void InstallRemoveMod_Click()
        {
            
        }

        private bool GetAvailableModViewFilter(object obj)
        {
            if (obj is ModObject mo)
            {
                return mo.Name != null && mo.Name.Contains(FilterText, StringComparison.InvariantCultureIgnoreCase);
            }
            else
                return false;
        }

        public async void OnLoaded()
        {
            AppStatus = "Looking for Steam...";

            if (!SteamHelper.IsSteamInstalled() || MordhauHelper.MordhauAppManifestFile == string.Empty)
            {
                MessageBox.Show("Unable to find Steam installation, please locate the Mordhau Folder yourself in the following dialog.", "Steam not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                ChooseMordhauFolderDialog();
                return;
            }

            AppStatus = "Looking for Mordhau App Manifest...";
            MordhauHelper.LocateMordhauAppManifestFile();

            MordhauHelper.MordhauInstallationFolder = SteamHelper.GetInstallDirFromAppManifest(MordhauHelper.MordhauAppManifestFile);

            AppStatus = "Reading mod.io api...";
            ModioHelper.LoadModioAccessToken(MordhauHelper.GetModioPath());
            await LoadMySubscriptions();
            await LoadAvailableMods();

            IsReadyForInput = true;

            AppStatus = "Ready";
        }

        private async Task LoadMySubscriptions()
        {
            int offset = 0;

            InstalledMods.Clear();

            GetModsResponse response = null;

            while (true)
            {
                response = await ModioHelper.GetUserSubscriptions(MordhauHelper.MORDHAU_MODIO_GAME_ID, ModioHelper.AccessToken, FilterText, offset);
                offset += response.ResultsAmount;

                foreach (var mod in response.ModObjects)
                {
                    InstalledMods.Add(mod);
                }

                if (offset >= response.TotalResults)
                    break;
            }
        }

        private async Task LoadAvailableMods()
        {
            int offset = 0;

            AvailableMods.Clear();

            GetModsResponse response = null;

            while(true)
            {
                response = await ModioHelper.GetModsAsync(MordhauHelper.MORDHAU_MODIO_GAME_ID, ModioHelper.AccessToken, FilterText, offset);
                offset += response.ResultsAmount;

                foreach (var mod in response.ModObjects)
                {
                    if (InstalledMods.Any(x => x.Id == mod.Id))
                        mod.IsInstalled = true;

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
