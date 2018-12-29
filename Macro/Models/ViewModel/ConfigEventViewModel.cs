using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Document;
using Macro.Extensions;
using System.ComponentModel;
using System.Collections.ObjectModel;

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

        public string EventType
        {
            get => DocumentHelper.Get(Label.EventType);
        }

        public string EventDataSet
        {
            get => DocumentHelper.Get(Label.EventDataSet);
        }
        public string MouseCoordinates
        {
            get => DocumentHelper.Get(Label.MouseCoordinates);
        }
        public string ConfigList
        {
            get => DocumentHelper.Get(Label.ConfigList);
        }
        public string Config
        {
            get => DocumentHelper.Get(Label.Config);
        }
        public string Save
        {
            get => DocumentHelper.Get(Label.Save);
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
