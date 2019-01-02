using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Utils;

namespace Macro.View
{
    /// <summary>
    /// MousePositionView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MousePositionView : Window
    {
        public Point MousePoint { get; private set; }
        public MousePositionView()
        {
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
                MousePoint = PointToScreen(position);
                e.Handled = true;
                this.Close();
            }
        }

        private void MousePositionView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                this.Close();
            }
            base.OnPreviewKeyDown(e);
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
    }
}
