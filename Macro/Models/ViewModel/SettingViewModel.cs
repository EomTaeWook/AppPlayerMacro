using Macro.Infrastructure;
using System.ComponentModel;
using System.Windows.Input;

namespace Macro.Models.ViewModel
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private Config _config;
        private ICommand _savePathButtonCmd;

        public SettingViewModel(IConfig config)
        {
            _config = new Config()
            {
                Language = config.Language,
                Period = config.Period,
                ProcessDelay = config.ProcessDelay,
                SavePath = config.SavePath,
                Similarity = config.Similarity
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
            get
            {
                return _savePathButtonCmd ?? (_savePathButtonCmd = new FolderBrowserDialogCmd());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
