using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using WPFCore;

namespace MordhauModManager.Model.Modio
{
    public class ModObject : ViewModelBase
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("game_id")]
        public int GameId { get; set; }

        [JsonProperty("submitted_by")]
        public UserObject SubmittedBy { get; set; }

        [JsonProperty("date_added")]
        public int DateAdded { get; set; }

        [JsonProperty("date_updated")]
        public int DateUpdated { get; set; }

        [JsonProperty("logo")]
        public LogoObject Logo { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("modfile")]
        public ModfileObject ModFileObject { get; set; }

        [JsonProperty("error")]
        public ErrorObject Error { get; set; }

        [JsonIgnore]
        private bool isInstalled;

        [JsonIgnore]
        public bool IsInstalled
        {
            get => isInstalled;
            set
            {
                isInstalled = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsInstalled)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(InstallRemoveText)));
            }
        }

        [JsonIgnore]
        private bool isInstalling;

        [JsonIgnore]
        public bool IsInstalling
        {
            get => isInstalling;
            set
            {
                isInstalling = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsInstalling)));
            }
        }

        [JsonIgnore]
        private bool isUpdateAvailable;

        [JsonIgnore]
        public bool IsUpdateAvailable
        {
            get => isUpdateAvailable;
            set
            {
                isUpdateAvailable = value;

                if (isUpdateAvailable)
                    InstallStatusImage = UpdateAvailableIcon;
                else
                    InstallStatusImage = InstalledIcon;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsUpdateAvailable)));
            }
        }

        [JsonIgnore]
        private int installProgress;

        [JsonIgnore]
        public int InstallProgress
        {
            get => installProgress;
            set
            {
                installProgress = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(InstallProgress)));
            }
        }

        [JsonIgnore]
        public BitmapImage LogoSource
        {
            get
            {
                return new BitmapImage(new Uri(Logo.Thumb_320x180));
            }
        }

        [JsonIgnore]
        private BitmapImage installStatusImage;

        [JsonIgnore]
        public BitmapImage InstallStatusImage
        {
            get => installStatusImage;
            set
            {
                installStatusImage = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(InstallStatusImage)));
            }
        }

        [JsonIgnore]
        public string InstallRemoveText
        {
            get => IsInstalled ? "Uninstall Mod" : "Install Mod";
        }

        [JsonIgnore]
        public BitmapImage InstallRemoveIcon
        {
            get => IsInstalled ? new BitmapImage(new Uri("pack://application:,,,/Icons/remove_icon.png")) : new BitmapImage(new Uri("pack://application:,,,/Icons/updateavailable_icon.png"));
        }

        [JsonIgnore]
        public static BitmapImage InstalledIcon = new BitmapImage(new Uri("pack://application:,,,/Icons/installed_icon.png"));

        [JsonIgnore]
        public static BitmapImage UpdateAvailableIcon = new BitmapImage(new Uri("pack://application:,,,/Icons/updateavailable_icon.png"));

        public ModObject()
        {
            InstallStatusImage = InstalledIcon;
        }
    }
}
