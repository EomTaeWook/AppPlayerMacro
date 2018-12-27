using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;
using Point = System.Windows.Point;
using Rect = Utils.Rect;

namespace Macro.View
{
    /// <summary>
    /// CaptureView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CaptureView : Window
    {
        private bool _isDrag;
        private Point _originPoint;

        public Bitmap CaptureImage { private set; get; }
        public CaptureView()
        {
            InitializeComponent();
            this.Loaded += CaptureView_Loaded;
        }

        private void CaptureView_Loaded(object sender, RoutedEventArgs e)
        {
            EventInit();
            Init();
        }
        private void Init()
        {
            WindowState = WindowState.Normal;

            var size = CaptureHelper.MonitorSize();
            Left = size.Left;
            Width = size.Width;

            Top = size.Top;
            Height = size.Height;
#if !DEBUG
            Topmost = true;
#endif
            this.Activate();
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
                this.Close();
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
                Canvas.SetLeft(dragBorder, current.X);
            else
                Canvas.SetLeft(dragBorder, origin.X);

            if (origin.Y - current.Y > 0)
                Canvas.SetTop(dragBorder, current.Y);
            else
                Canvas.SetTop(dragBorder, origin.Y);

            if (current.X > origin.X)
                dragBorder.Width = current.X - origin.X;
            else
                dragBorder.Width = origin.X - current.X;

            if (current.Y > origin.Y)
                dragBorder.Height = current.Y - origin.Y;
            else
                dragBorder.Height = origin.Y - current.Y;
        }

        private void CaptureZone_MouseLeave(object sender, MouseEventArgs e)
        {
            if(_isDrag)
            {
                if(this.IsVisible)
                {
                    SaveCapture(new Rect()
                    {
                        Left = (int)Canvas.GetLeft(dragBorder),
                        Bottom = (int)dragBorder.Height + (int)Canvas.GetTop(dragBorder),
                        Top = (int)Canvas.GetTop(dragBorder),
                        Right = (int)dragBorder.Width + (int)Canvas.GetLeft(dragBorder),
                    });
                    this.Close();
                }
                e.Handled = true;
                return;
            }
            _isDrag = false;
            e.Handled = true;
        }
        private void SaveCapture(Rect rect)
        {
            CaptureImage = CaptureHelper.Capture(rect);
        }
    }
}
