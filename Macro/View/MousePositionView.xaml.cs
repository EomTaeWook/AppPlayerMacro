using Macro.Infrastructure;
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
        private MonitorInfo _monitorInfo;
        public MousePositionView(MonitorInfo monitorInfo)
        {
            _monitorInfo = monitorInfo;
            InitializeComponent();
            Loaded += MousePositionView_Loaded;
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
            MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(IsVisible)
            {
                e.Handled = true;
                NotifyHelper.InvokeNotify(EventType.MousePointDataBind, new MousePointEventArgs()
                {
                    MouseTriggerInfo = new Models.MouseTriggerInfo()
                    {
                        MouseInfoEventType = Models.MouseEventType.LeftClick,
                        StartPoint = PointToScreen(e.GetPosition(this))
                    },
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
                NotifyHelper.InvokeNotify(EventType.MousePointDataBind, new MousePointEventArgs()
                {
                    MouseTriggerInfo = new Models.MouseTriggerInfo()
                    {
                        MouseInfoEventType = Models.MouseEventType.None,
                    },
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
        }
    }
}
