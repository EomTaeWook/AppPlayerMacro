using Macro.Extensions;
using Macro.Models;
using Macro.Models.ViewModel;
using Macro.UI;
using Macro.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Macro.Infrastructure.Impl
{
    public abstract class BaseEventConfigView<ViewModel> : UserControl 
                                                where ViewModel : IBaseNotifyEventConfigViewModel
    {

        protected readonly List<MousePositionView> _mousePointViews;
        protected bool _isDrag;
        protected readonly ObservableCollection<KeyValuePair<RepeatType, string>> _repeatItems;

        public BaseEventConfigView()
        {
            
            
            _mousePointViews = new List<MousePositionView>();

            _repeatItems = new ObservableCollection<KeyValuePair<RepeatType, string>>();

            _mousePointViews = new List<MousePositionView>();

            

        }
        protected void ShowMousePoisitionView()
        {
            foreach (var item in _mousePointViews)
                item.ShowActivate();
        }
    }
}
