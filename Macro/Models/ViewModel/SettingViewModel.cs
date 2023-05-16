using Macro.Command;
using System.ComponentModel;
using System.Windows.Input;

namespace Macro.Models.ViewModel
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private Config _config;
        private ICommand _savePathButtonCmd;

        public SettingViewModel(Config config)
        {
            _config = new Config()
            {
                Language = config.Language,
                Period = config.Period,
                ItemDelay = config.ItemDelay,
                SavePath = config.SavePath,
                Similarity = config.Similarity,
                DragDelay = config.DragDelay,
                SearchImageResultDisplay = config.SearchImageResultDisplay,
                VersionCheck = config.VersionCheck,
                ProcessLocationX = config.ProcessLocationX,
                ProcessLocationY = config.ProcessLocationY  
            };
        }
        public Config Config
        {
            get => _config;
            set
            {
                _config = value;
                OnPropertyChanged("Config");
            }
        }

        public ICommand SavePathButtonCmd
        {
            get => _savePathButtonCmd ?? (_savePathButtonCmd = new FolderBrowserDialogCommand());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
