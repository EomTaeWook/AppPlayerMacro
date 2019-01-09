using Macro.Extensions;
using Macro.Models.Event;
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
        public event DataBindingHander DataBinding;
        public delegate void DataBindingHander(object sender, CaptureArgs args);

        private bool _isDrag;
        private Point _originPoint;
        private MonitorInfo _monitorInfo;
        private Border _dummyBorder, _dragBorder;

        public CaptureView(MonitorInfo monitorInfo)
        {
            _monitorInfo = monitorInfo;
            _dummyBorder = new Border();

            InitializeComponent();
            Loaded += CaptureView_Loaded;
        }

        private void CaptureView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void Init()
        {
            _dummyBorder.BorderBrush = Brushes.Blue;
            _dummyBorder.BorderThickness = new Thickness(1);
            _dummyBorder.Background = Brushes.LightBlue;
            _dummyBorder.Opacity = 1;
            _dummyBorder.CornerRadius = new CornerRadius(1);

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
        public void ShowActivate()
        {
            Clear();
            Show();
            Activate();
        }
        private void CaptureView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                e.Handled = true;
                DataBinding?.Invoke(this, new CaptureArgs()
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
            if (_isDrag)
            {
                Point currentPoint = e.GetPosition(captureZone);
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
            if(_isDrag && IsVisible)
            {
                WindowState = WindowState.Minimized;
                DataBinding?.Invoke(this, new CaptureArgs()
                {
                    MonitorInfo = _monitorInfo,
                    CaptureImage = DisplayHelper.Capture(_monitorInfo, new Rect()
                    {
                        Left = (int)Canvas.GetLeft(_dragBorder),
                        Bottom = (int)_dragBorder.Height + (int)Canvas.GetTop(_dragBorder),
                        Top = (int)Canvas.GetTop(_dragBorder),
                        Right = (int)_dragBorder.Width + (int)Canvas.GetLeft(_dragBorder),
                    })
                });
                e.Handled = true;
                WindowState = WindowState.Maximized;
            }
            _isDrag = false;
        }
    }
}
