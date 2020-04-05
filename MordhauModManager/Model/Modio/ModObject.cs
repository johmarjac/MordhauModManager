using Newtonsoft.Json;
using System.ComponentModel;
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

        private bool isInstalled;
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

        public string InstallRemoveText
        {
            get => IsInstalled ? "Remove Mod" : "Install Mod";
        }
    }
}
