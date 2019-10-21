using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace Macro.View
{
    /// <summary>
    /// CommonContentView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommonContentView : BaseContentView
    {
        private readonly List<CaptureView> _captureViews;
        public CommonContentView()
        {
            _captureViews = new List<CaptureView>();
            InitializeComponent();
            InitEvent();
            Init();
        }
        private void InitEvent()
        {
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            btnAddSameContent.Click += Button_Click;
            btnCapture.Click += Button_Click;

            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;

            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _captureViews)
            {
                item.Close();
            }
            _captureViews.Clear();
        }

        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs e)
        {
            if (e.CaptureViewMode != CaptureViewMode.Common)
            {
                return;
            }

            foreach (var item in _captureViews)
            {
                item.Hide();
            }
            if (e.CaptureImage != null)
            {
                var capture = e.CaptureImage;
                canvasCaptureImage.Background = new ImageBrush(capture.ToBitmapSource());
                _bitmap = new Bitmap(capture, capture.Width, capture.Height);
            }
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
        private void NotifyHelper_SelectTreeViewChanged(SelctTreeViewItemChangedEventArgs e)
        {            
            if (e.TreeViewItem == null)
            {
                Clear();
                return;
            }
            else if (e.TreeViewItem.DataContext<EventTriggerModel>() == null)
            {
                return;
            }
            else
            {
                var model = e.TreeViewItem.DataContext<EventTriggerModel>();
                btnDelete.Visibility = Visibility.Visible;
                btnAddSameContent.Visibility = Visibility.Visible;

                _bitmap = model.Image;
                canvasCaptureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                Capture();
            }
            else if (btn.Equals(btnSave))
            {
                var model = configView.CurrentTreeViewItem.DataContext<EventTriggerModel>();
                model.Image = _bitmap;
                if (model.EventType == EventType.RelativeToImage)
                {
                    model.MouseTriggerInfo.StartPoint = new Point(configView.RelativePosition.X, configView.RelativePosition.Y);
                }
                
                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if (btn.Equals(btnDelete))
            {
                var model = configView.CurrentTreeViewItem.DataContext<EventTriggerModel>();
                NotifyHelper.InvokeNotify(NotifyEventType.Delete, new DeleteEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if(btn.Equals(btnAddSameContent))
            {
                var item = configView.CopyCurrentItem();
                if (item == null)
                    return;
                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = item,
                });
            }
        }
    }
}
