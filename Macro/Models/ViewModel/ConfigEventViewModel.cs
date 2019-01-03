using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public class ConfigEventViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ConfigEventModel> _configs;
        private ConfigEventModel _config;
        public ConfigEventViewModel()
        {
            _configs = new ObservableCollection<ConfigEventModel>();
        }

        private void Configs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("ConfigSaves");
        }
        public ObservableCollection<ConfigEventModel> ConfigSaves
        {
            get => _configs;
            private set
            {
                _configs = value;
                OnPropertyChanged("ConfigSaves");
            }
        }

        public ConfigEventModel ConfigData
        {
            get => _config;
            set
            {
                _config = value;
                OnPropertyChanged("ConfigData");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }
}
