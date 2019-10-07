using Macro.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Models.ViewModel
{
    public class GameEventConfigViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TreeGridViewItem _currentItem;

        private ObservableCollection<EventTriggerModel> _triggerSaves;

        public GameEventConfigViewModel()
        {
            _triggerSaves = new ObservableCollection<EventTriggerModel>();
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
    
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
