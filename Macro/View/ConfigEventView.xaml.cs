using Macro.Extensions;
using Macro.Models;
using Macro.Models.ViewModel;
using MahApps.Metro.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity;
using Utils;
using System.Linq;

namespace Macro.View
{
    /// <summary>
    /// ConfigEventView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigEventView : UserControl
    {
        private ConfigEventModel _dummy;
        public event SelectConfigDataHandler SelectData;
        public delegate void SelectConfigDataHandler(ConfigEventModel model);
        public ConfigEventView()
        {
            _dummy = new ConfigEventModel();
            DataContext = Singleton<UnityContainer>.Instance.Resolve<ConfigEventViewModel>();
            Model = _dummy;
            InitializeComponent();
            
            Loaded += ConfigEventView_Loaded;
            grdSaves.SelectionChanged += GrdSaves_SelectionChanged;
            PreviewKeyDown += ConfigEventView_PreviewKeyDown;
        }

        private void ConfigEventView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                grdSaves.SelectedItem = null;
                Model = _dummy;
                SelectData(null);
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void GrdSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as DataGrid).SelectedItem is ConfigEventModel item)
            {
                Model = new ConfigEventModel(item);
                SelectData(item);
                if(Model.EventType == EventType.Keyboard)
                {
                    btnMouseCoordinate.Visibility = Visibility.Collapsed;
                    txtKeyboardCmd.Visibility = Visibility.Visible;
                }
                else if(Model.EventType == EventType.Mouse)
                {
                    btnMouseCoordinate.Visibility = Visibility.Visible;
                    txtKeyboardCmd.Visibility = Visibility.Collapsed;
                }
                e.Handled = true;
            }
        }
        private void ConfigEventView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void Init()
        {
            grdSaves.ItemsSource = (DataContext as ConfigEventViewModel).ConfigSaves;
        }
        private void EventInit()
        {
            var radioButtons = this.FindChildren<RadioButton>();
            foreach(var button in radioButtons)
            {
                button.Click += RadioButton_Click;
            }
            btnMouseCoordinate.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Model == _dummy)
            {
                Model = new ConfigEventModel();
            }
            if (sender.Equals(btnMouseCoordinate))
            {
                var mousePosition = new MousePositionView();
                mousePosition.ShowDialog();
                Model.MousePoint = mousePosition.MousePoint;
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model == _dummy)
            {
                Model = new ConfigEventModel();
            }

            if (sender.Equals(rbMouse))
            {
                btnMouseCoordinate.Visibility = Visibility.Visible;
                txtKeyboardCmd.Visibility = Visibility.Collapsed;

                Model.EventType = EventType.Mouse;
            }
            else if (sender.Equals(rbKeyboard))
            {
                btnMouseCoordinate.Visibility = Visibility.Collapsed;
                txtKeyboardCmd.Visibility = Visibility.Visible;

                Model.EventType = EventType.Keyboard;
            }
        }
        public void InsertModel(ConfigEventModel model)
        {
            Dispatcher.Invoke(() =>
            {
                ((ConfigEventViewModel)DataContext).ConfigSaves.Add(model);
                if (Model != _dummy)
                {
                    Model = _dummy;
                }
            });
        }
        public void RemoveModel(ConfigEventModel model)
        {
            Dispatcher.Invoke(() =>
            {
                if(((ConfigEventViewModel)DataContext).ConfigSaves.Remove(model.Index))
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
        public ConfigEventModel Model
        {
            get => ((ConfigEventViewModel)DataContext).ConfigData;
            private set => ((ConfigEventViewModel)DataContext).ConfigData = value;
        }
    }
}
