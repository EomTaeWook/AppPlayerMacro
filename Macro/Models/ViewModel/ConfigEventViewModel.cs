using Macro.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public class ConfigEventViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<EventTriggerModel> _triggerSaves;
        private TreeGridViewItem _currentItem;
        private PointModel _pointModel;
        public ConfigEventViewModel()
        {
            _triggerSaves = new ObservableCollection<EventTriggerModel>();
        }

        private void Configs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("TriggerSaves");
        }
        public ObservableCollection<EventTriggerModel> TriggerSaves
        {
            get => _triggerSaves;
            private set
            {
                _triggerSaves = value;
                OnPropertyChanged("TriggerSaves");
            }
        }

        public TreeGridViewItem CurrentTreeViewItem
        {
            get => _currentItem;
            set
            {
                _currentItem = value;
                OnPropertyChanged("CurrentTreeViewItem");
            }
        }
        public PointModel RelativePosition
        {
            get => _pointModel;
            set
            {
                _pointModel = value;
                OnPropertyChanged("RelativePosition");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }
}
