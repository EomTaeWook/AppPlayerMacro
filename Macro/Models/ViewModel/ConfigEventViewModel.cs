using Macro.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public class ConfigEventViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<EventTriggerModel> _triggerSaves;
        private EventTriggerModel _trigger;
        private TreeListViewItem _dragItem;
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

        public EventTriggerModel Trigger
        {
            get => _trigger;
            set
            {
                _trigger = value;
                OnPropertyChanged("Trigger");
            }
        }

        public TreeListViewItem DragTreeItem
        {
            get => _dragItem;
            set
            {
                _dragItem = value;
                OnPropertyChanged("DragTreeItem");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }
}
