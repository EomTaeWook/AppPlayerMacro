using Macro.Models;
using Macro.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Macro.Models.ViewModel
{
    public interface IBaseNotifyEventConfigViewModel : INotifyPropertyChanged
    {
        TreeGridViewItem CurrentTreeViewItem { get; set; }
        PointModel RelativePosition { get; set; }
    }

    public abstract class BaseEventConfigViewModel<T> : IBaseNotifyEventConfigViewModel where T : BaseEventTriggerModel<T>
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<T> _triggerSaves;
        private TreeGridViewItem _currentItem;
        private PointModel _pointModel;
        private double _width;
        private double _height;

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
        public double Width
        {
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
            get => _width;
        }
        public double Height
        {
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
            get => _height;
        }
    }
}
