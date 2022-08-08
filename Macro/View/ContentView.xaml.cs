using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Infrastructure.Serialize;
using Macro.Models;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utils;
using Point = System.Windows.Point;

namespace Macro.View
{
    /// <summary>
    /// CommonContentView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentView : UserControl
    {
        private List<CaptureView> _captureViews = new List<CaptureView>();
        private Bitmap _bitmap;
        
        public ContentView()
        {
            InitializeComponent();
            InitEvent();
        }
        public void Clear()
        {
            btnDelete.Visibility = Visibility.Collapsed;
            btnAddSameContent.Visibility = Visibility.Collapsed;
            _bitmap = null;
            canvasCaptureImage.Background = System.Windows.Media.Brushes.White;
            //eventConfigView.Clear();
        }
        public void Capture()
        {
            Clear();
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            foreach (var item in _captureViews)
            {
                item.ShowActivate();
            }
        }
        
       
        public void SaveDataBind(List<EventTriggerModel> saves)
        {
            Dispatcher.Invoke(() =>
            {
                (eventConfigView.DataContext as Models.ViewModel.EventConfigViewModel).TriggerSaves.Clear();
                foreach (var item in saves)
                {
                    (eventConfigView.DataContext as Models.ViewModel.EventConfigViewModel).TriggerSaves.Add(item);
                }
            });
        }
        public void DrawCaptureImage(Bitmap bmp)
        {
            Dispatcher.Invoke(() =>
            {
                canvasCaptureImage.Background = new ImageBrush(bmp.ToBitmapSource());
            });
        }
        private void InitEvent()
        {
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            btnAddSameContent.Click += Button_Click;
            btnCapture.Click += Button_Click;

            Loaded += ContentView_Loaded;

            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;

            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
        }

        private void ContentView_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
            }
            Clear();
        }
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach(var item in _captureViews)
            {
                item.Close();
            }
        }

        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs e)
        {
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
        public Task Delete(object state)
        {
            if (state is string path)
            {
                eventConfigView.RemoveCurrentItem();

                if (File.Exists(path))
                {
                    File.Delete(path);
                    using (var fs = new FileStream(path, FileMode.CreateNew))
                    {
                        foreach (var data in eventConfigView.DataContext<Models.ViewModel.EventConfigViewModel>().TriggerSaves)
                        {
                            var bytes = ObjectSerializer.SerializeObject(data);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                        fs.Close();
                    }
                }
            }
            return Task.CompletedTask;
        }
        public void Save(string path)
        {
            eventConfigView.InsertCurrentItem();

            var viewModel = eventConfigView.DataContext as Models.ViewModel.EventConfigViewModel;

            FileManager.Instance.Save(path, viewModel.TriggerSaves);
            
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
                var dataContext = eventConfigView.GetDataContext();
                var model = dataContext.CurrentTreeViewItem.DataContext<EventTriggerModel>();
                model.Image = _bitmap;
                if (model.EventType == EventType.RelativeToImage)
                {
                    model.MouseTriggerInfo.StartPoint = new Point(dataContext.RelativePosition.X, dataContext.RelativePosition.Y);
                }
                
                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if(btn.Equals(btnAddSameContent))
            {
                var item = eventConfigView.CopyCurrentItem();
                if (item == null)
                    return;
                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = item,
                });
            }
            else if(btn.Equals(btnDelete))
            {
                var model = eventConfigView.GetDataContext();
                NotifyHelper.InvokeNotify(NotifyEventType.Delete, new DeleteEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model.CurrentTreeViewItem.DataContext<EventTriggerModel>(),
                });
            }
        }
    }
}
