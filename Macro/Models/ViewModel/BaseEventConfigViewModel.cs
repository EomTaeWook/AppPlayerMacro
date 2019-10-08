using Macro.Models;
using Macro.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public interface IBaseNotifyEventConfigViewModel : INotifyPropertyChanged
    {
    }

    public abstract class BaseEventConfigViewModel<T> : IBaseNotifyEventConfigViewModel where T: BaseEventTriggerModel<T>
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<T> _triggerSaves;
        private TreeGridViewItem _currentItem;
        private PointModel _pointModel;

        public BaseEventConfigViewModel()
        {
           _triggerSaves = new ObservableCollection<T>();
        }
        public ObservableCollection<T> TriggerSaves
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
    }
}
