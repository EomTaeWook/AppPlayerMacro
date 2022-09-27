using Macro.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public class EventSettingViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<EventTriggerModel> triggerSaves = new ObservableCollection<EventTriggerModel>();
        private TreeGridViewItem currentItem = new TreeGridViewItem();
        private PointModel pointModel = new PointModel();
        private double width;
        private double height;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<EventTriggerModel> TriggerSaves
        {
            get => triggerSaves;
            private set
            {
                triggerSaves = value;
                OnPropertyChanged("TriggerSaves");
            }
        }
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => currentItem;
            set
            {
                currentItem = value;
                OnPropertyChanged("CurrentTreeViewItem");
            }
        }
        public PointModel RelativePosition
        {
            get => pointModel;
            set
            {
                pointModel = value;
                OnPropertyChanged("RelativePosition");
            }
        }
        public double Width
        {
            set
            {
                width = value;
                OnPropertyChanged("Width");
            }
            get => width;
        }
        public double Height
        {
            set
            {
                height = value;
                OnPropertyChanged("Height");
            }
            get => height;
        }
    }
}
