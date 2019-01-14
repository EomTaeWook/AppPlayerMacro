using Macro.Extensions;
using Macro.Models;
using Macro.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Utils;

namespace Macro.View
{
    public partial class ConfigEventView : UserControl
    {
        public EventTriggerModel Model
        {
            get => ((ConfigEventViewModel)DataContext).Trigger;
            private set => ((ConfigEventViewModel)DataContext).Trigger = value;
        }
        public List<EventTriggerModel> TriggerSaves
        {
            get => ((ConfigEventViewModel)DataContext).TriggerSaves.ToList();
        }

        private List<MousePositionView> _mousePointViews;
        private EventTriggerModel _dummy;
        public ConfigEventView()
        {
            _dummy = new EventTriggerModel();
            _mousePointViews = new List<MousePositionView>();
            DataContext = new ViewModelLocator().ConfigEventViewModel;
            Model = _dummy;
            InitializeComponent();

            Loaded += ConfigEventView_Loaded;
        }

        private void Init()
        {
            grdSaves.ItemsSource = (DataContext as ConfigEventViewModel).TriggerSaves;
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _mousePointViews.Add(new MousePositionView(item));
            }
        }

        public void ShowMousePoisitionView()
        {
            foreach (var item in _mousePointViews)
                item.ShowActivate();
        }
        public void InsertModel(EventTriggerModel model)
        {
            Dispatcher.Invoke(() =>
            {
                ((ConfigEventViewModel)DataContext).TriggerSaves.Add(model);
                if (Model != _dummy)
                {
                    Model = _dummy;
                }
            });
        }
        public void RemoveModel(EventTriggerModel model)
        {
            Dispatcher.Invoke(() =>
            {
                if (((ConfigEventViewModel)DataContext).TriggerSaves.Remove(model.Index))
                {
                    if (Model != _dummy)
                    {
                        Model = _dummy;
                    }
                }
            });
        }
        public void Clear()
        {
            if (Model != _dummy)
            {
                Model = _dummy;
            }
            grdSaves.SelectedItem = null;
        }

        private DataGridRow GetRowItem(int index)
        {
            if (grdSaves.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return grdSaves.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private bool IsMouseOnTargetRow(Visual theTarget, GetDragDropPosition pos)
        {
            Rect posBounds = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point theMousePos = pos((IInputElement)theTarget);
            return posBounds.Contains(theMousePos);
        }

        private int CurrentRowIndex(GetDragDropPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < grdSaves.Items.Count; i++)
            {
                DataGridRow item = GetRowItem(i);
                if (IsMouseOnTargetRow(item, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }
    }
}
