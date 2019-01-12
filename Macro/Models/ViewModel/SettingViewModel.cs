using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public class SettingViewModel : INotifyPropertyChanged
    {
        private Config _config;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
