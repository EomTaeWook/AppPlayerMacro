using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utils;
using Image = Macro.Extensions.Image;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Process> _processes;
        private bool _isDrag;
        private Point _originPoint;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            _processes = Process.GetProcesses().ToList();
            combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();

            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
            captureZone.MouseLeftButtonDown += CaptureZone_MouseLeftButtonDown; ;
            captureZone.MouseMove += CaptureZone_MouseMove;
            captureZone.MouseLeave += CaptureZone_MouseLeave;
            captureZone.MouseLeftButtonUp += CaptureZone_MouseLeave;
        }
        private void CaptureZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrag = true;
            _originPoint = e.GetPosition(captureZone);
            e.Handled = true;
        }

        private void CaptureZone_MouseMove(object sender, MouseEventArgs e)
        {
            if(_isDrag)
            {
                Point currentPoint = e.GetPosition(captureZone);
                UpdateDragSelectionRect(_originPoint, currentPoint);
                e.Handled = true;
            }
        }
        private void UpdateDragSelectionRect(Point origin, Point current)
        {
            if(origin.X - current.X > 0)
                Canvas.SetLeft(dragBorder, current.X);
            else
                Canvas.SetLeft(dragBorder, origin.X);

            if (origin.Y - current.Y > 0)
                Canvas.SetTop(dragBorder, current.Y);
            else
                Canvas.SetTop(dragBorder, origin.Y);

            if(current.X > origin.X)
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
            _isDrag = false;
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = combo_process.SelectedValue;
            if (item == null)
                return;

            var btn = sender as Button;
            if (btn.Equals(btnCapture) && TryCapture(item))
            {
                Application.Current.MainWindow.Activate();
            }
            else if(btn.Equals(btnRefresh))
                _processes = Process.GetProcesses().ToList();
        }
        private bool TryCapture(object item)
        {
            var process = _processes.Where(r => r.ProcessName == item.ToString()).FirstOrDefault();
            if (process == null)
                return false;

            var imageBrush = new ImageBrush(Image.ToBitmapSource(CaptureHelper.Capture(process)));
            captureZone.Background = imageBrush;
            return true;
        }
        
    }
}
