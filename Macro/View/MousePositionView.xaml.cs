using Macro.Models.Event;
using System.Windows;
using System.Windows.Input;
using Utils.Infrastructure;

namespace Macro.View
{
    /// <summary>
    /// MousePositionView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MousePositionView : Window
    {
        public event DataBindingHander DataBinding;
        public delegate void DataBindingHander(object sender, MousePointArgs args);

        private MonitorInfo _monitorInfo;
        public MousePositionView(MonitorInfo monitorInfo)
        {
            _monitorInfo = monitorInfo;
            InitializeComponent();
            this.Loaded += MousePositionView_Loaded;
        }

        private void MousePositionView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
            Activate();
        }
        private void EventInit()
        {
            PreviewKeyDown += MousePositionView_PreviewKeyDown;
            this.MouseLeftButtonDown += PointZone_MouseLeftButtonDown;
        }

        private void PointZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.IsVisible)
            {
                var position = e.GetPosition(this);
                
                e.Handled = true;
                DataBinding? .Invoke(this, new MousePointArgs()
                {
                    MousePoint = position,
                    MonitorInfo = _monitorInfo
                });
            }
        }
        public void ShowActivate()
        {
            Show();
            Activate();
        }
        private void MousePositionView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DataBinding?.Invoke(this, new MousePointArgs()
                {
                    MousePoint = null,
                    MonitorInfo = _monitorInfo
                });
                e.Handled = true;
            }
            base.OnPreviewKeyDown(e);
        }

        private void Init()
        {
#if !DEBUG
            Topmost = true;
#endif
            Left = _monitorInfo.Rect.Left;
            Width = _monitorInfo.Rect.Width;
            Top = _monitorInfo.Rect.Top;
            Height = _monitorInfo.Rect.Height;
            WindowState = WindowState.Maximized;
            Activate();
        }
    }
}
