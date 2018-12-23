using Macro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utils;
using Utils.Document;
using Image = Macro.Extensions.Image;
using Macro.Extensions;
using Unity;
using MahApps.Metro.Controls;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<Process> _processes;
        private bool _isDrag;
        private Point _originPoint;
        private IConfig _config;
        public MainWindow()
        {
            InitializeComponent();
            _config = Singleton<UnityContainer>.Instance.Resolve<IConfig>();
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
            if(captureZone.Background.GetType() != typeof(ImageBrush))
                return;

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
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                if (TryCapture())
                    Application.Current.MainWindow.Activate();
                else
                    MessageBox.Show(Singleton<LabelDocument>.Instance.Get(_config.Language, "FailCapture"));
            }
            else if(btn.Equals(btnRefresh))
            {
                _processes = Process.GetProcesses().ToList();
                combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();
            }
        }
        private bool TryCapture()
        {
            var item = combo_process.SelectedValue;
            if (item == null)
                return false;
            var process = _processes.Where(r => r.ProcessName == item.ToString()).FirstOrDefault();
            if (process == null)
                return false;
            var bitmap = CaptureHelper.Capture(process);
            if (bitmap != null)
            {
                var imageBrush = new ImageBrush(Image.ToBitmapSource(bitmap));
                captureZone.Background = imageBrush;
                return true;
            }
            return false;
        }
    }
}
