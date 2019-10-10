using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;
using Utils.Infrastructure;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rect = Utils.Infrastructure.Rect;

namespace Macro.View
{
    /// <summary>
    /// CaptureView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CaptureView : Window
    {
        private bool _isDrag;
        private Point _originPoint;
        private readonly MonitorInfo _monitorInfo;
        private readonly Border _dummyBorder;
        private Border _dragBorder;
        private Point _factor;
        private CaptureViewMode _captureViewMode;

        public CaptureView(MonitorInfo monitorInfo)
        {
            _monitorInfo = monitorInfo;
            _captureViewMode = CaptureViewMode.Common;

            _dummyBorder = new Border
            {
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(1),
                Background = Brushes.LightBlue,
                SnapsToDevicePixels = true,
                Opacity = 1,
                CornerRadius = new CornerRadius(1)
            };
            var systemDPI = NativeHelper.GetSystemDPI();
            _factor.X = 1.0F * _monitorInfo.Dpi.X / systemDPI.X;
            _factor.Y = 1.0F *_monitorInfo.Dpi.Y / systemDPI.Y;
            InitializeComponent();
            Loaded += CaptureView_Loaded;
        }

        public void ShowActivate()
        {
            Clear();
            Show();
            Activate();
        }
        public void Setting(CaptureViewMode captureViewMode)
        {
            _captureViewMode = captureViewMode;
        }
        private void CaptureView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void Init()
        {
            Clear();

#if !DEBUG
            Topmost = true;
#endif

            Left = _monitorInfo.Rect.Left;
            Width = _monitorInfo.Rect.Width;
            Top = _monitorInfo.Rect.Top;
            Height = _monitorInfo.Rect.Height;

            WindowState = WindowState.Maximized;
        }
        private void Clear()
        {
            captureZone.Children.Clear();
            _dragBorder = _dummyBorder.Clone();
            captureZone.Children.Add(_dragBorder);
        }
        private void EventInit()
        {
            captureZone.MouseLeftButtonDown += CaptureZone_MouseLeftButtonDown;
            captureZone.MouseMove += CaptureZone_MouseMove;
            captureZone.MouseLeave += CaptureZone_MouseLeave;
            captureZone.MouseLeftButtonUp += CaptureZone_MouseLeave;

            PreviewKeyDown += CaptureView_PreviewKeyDown;
        }
        private void CaptureView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                e.Handled = true;
                NotifyHelper.InvokeNotify(NotifyEventType.ScreenCaptureDataBInd, new CaptureEventArgs()
                {
                    MonitorInfo = _monitorInfo,
                    CaptureImage = null
                });
            }
            base.OnPreviewKeyDown(e);
        }

        private void CaptureZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = true;
            _originPoint = e.GetPosition(captureZone);
            e.Handled = true;
        }
        private void CaptureZone_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrag && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(captureZone);
                UpdateDragSelectionRect(_originPoint, currentPoint);
                e.Handled = true;
            }
        }
        private void UpdateDragSelectionRect(Point origin, Point current)
        {
            if (origin.X - current.X > 0)
                Canvas.SetLeft(_dragBorder, current.X);
            else
                Canvas.SetLeft(_dragBorder, origin.X);

            if (origin.Y - current.Y > 0)
                Canvas.SetTop(_dragBorder, current.Y);
            else
                Canvas.SetTop(_dragBorder, origin.Y);

            if (current.X > origin.X)
                _dragBorder.Width = current.X - origin.X;
            else
                _dragBorder.Width = origin.X - current.X;

           if (current.Y > origin.Y)
                _dragBorder.Height = current.Y - origin.Y;
            else
                _dragBorder.Height = origin.Y - current.Y;
        }

        private void CaptureZone_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDrag && IsVisible)
            {
                WindowState = WindowState.Minimized;
                int left = (int)(Canvas.GetLeft(_dragBorder) * _factor.X);
                int top = (int)(Canvas.GetTop(_dragBorder) * _factor.Y);
                int width = (int)(_dragBorder.Width * _factor.X);
                int height = (int)(_dragBorder.Height * _factor.Y);
                var rect = new Rect
                {
                    Left = left,
                    Right = width + left,
                    Bottom = top + height,
                    Top = top
                };
                NotifyHelper.InvokeNotify(NotifyEventType.ScreenCaptureDataBInd, new CaptureEventArgs()
                {
                    CaptureViewMode = _captureViewMode,
                    MonitorInfo = _monitorInfo,
                    CaptureImage = DisplayHelper.Capture(_monitorInfo, rect),
                    Position = rect
                });
                e.Handled = true;
                WindowState = WindowState.Maximized;
            }
            _isDrag = false;
        }
    }
}
