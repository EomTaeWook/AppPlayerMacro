using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using Utils;
using Point = System.Windows.Point;
using MahApps.Metro.Controls;
using Utils.Document;
using System.Diagnostics;

namespace Macro.View
{
    /// <summary>
    /// GameContentVIew.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameContentView : BaseContentView
    {
        private readonly List<CaptureView> _captureViews;
        private Dictionary<string, ColorModel> _colorDatas;


        public GameContentView()
        {
            _captureViews = new List<CaptureView>();
            _dummyRect = new Utils.Infrastructure.Rect();

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
            btnHpCapture.Click += Button_Click;
            btnMpCapture.Click += Button_Click;

            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;
            NotifyHelper.ComboProcessChanged += NotifyHelper_ComboProcessChanged;

            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
        }

        private void NotifyHelper_ComboProcessChanged(ComboProcessChangedEventArgs obj)
        {
            _currentProcess = obj.Process;
        }

        private void NotifyHelper_SelectTreeViewChanged(SelctTreeViewItemChangedEventArgs e)
        {
            if (e.TreeViewItem == null)
            {
                Clear();
                return;
            }
            else if (e.TreeViewItem.DataContext<GameEventTriggerModel>() == null)
            {
                return;
            }
            else
            {
                var model = e.TreeViewItem.DataContext<GameEventTriggerModel>();
                btnDelete.Visibility = Visibility.Visible;
                btnAddSameContent.Visibility = Visibility.Visible;

                _bitmap = model.Image;
                canvasCaptureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
        }
        private void NotifyHelper_ScreenCaptureDataBind(CaptureEventArgs e)
        {
            if (e.CaptureViewMode != CaptureViewMode.Game &&
                e.CaptureViewMode != CaptureViewMode.HP &&
                e.CaptureViewMode != CaptureViewMode.Mp)
            {
                return;
            }

            foreach (var item in _captureViews)
            {
                item.Hide();
            }
            if (e.CaptureImage == null)
                return;

            var capture = e.CaptureImage;

            if (e.CaptureViewMode == CaptureViewMode.Game)
            {
                canvasCaptureImage.Background = new ImageBrush(capture.ToBitmapSource());
                _bitmap = new Bitmap(capture, capture.Width, capture.Height);
            }
            else if(e.CaptureViewMode == CaptureViewMode.HP)
            {
                var processPosition = new Utils.Infrastructure.Rect();

                NativeHelper.GetWindowRect(_currentProcess.MainWindowHandle, ref processPosition);

                var roiPosition = CalculatorRoiPosition(processPosition, e.Position, e.MonitorInfo);
                if (roiPosition.Equals(_dummyRect))
                {
                    return;
                }

                _hpRoiPosition = new RoiPositionModel()
                {
                    MonitorInfo = e.MonitorInfo,
                    RoiPosition = roiPosition,
                };
                canvasCaptureHp.Background = new ImageBrush(capture.ToBitmapSource());
            }
           else if(e.CaptureViewMode == CaptureViewMode.Mp)
            {
                var processPosition = new Utils.Infrastructure.Rect();

                NativeHelper.GetWindowRect(_currentProcess.MainWindowHandle, ref processPosition);

                var roiPosition = CalculatorRoiPosition(processPosition, e.Position, e.MonitorInfo);
                if (roiPosition.Equals(_dummyRect))
                {
                    return;
                }
                _mpRoiPosition = new RoiPositionModel()
                {
                    MonitorInfo = e.MonitorInfo,
                    RoiPosition = roiPosition,
                };
                canvasCaptureMp.Background = new ImageBrush(capture.ToBitmapSource());
            }
            
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _captureViews)
            {
                item.Close();
            }
            _captureViews.Clear();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                Capture(CaptureViewMode.Game);
            }
            else if(btn.Equals(btnHpCapture))
            {
                if(_currentProcess == null)
                {
                    (Application.Current.MainWindow as MetroWindow).MessageShow("Error", DocumentHelper.Get(Message.FailedPreconditionSelectProcess));
                    return;
                }
                Capture(CaptureViewMode.HP);
            }
            else if(btn.Equals(btnMpCapture))
            {
                if (_currentProcess == null)
                {
                    (Application.Current.MainWindow as MetroWindow).MessageShow("Error", DocumentHelper.Get(Message.FailedPreconditionSelectProcess));
                    return;
                }
                Capture(CaptureViewMode.Mp);
            }
            else if (btn.Equals(btnSave))
            {
                var model = gameConfigView.CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                if(model.ImageSearchRequired)
                {
                    model.Image = _bitmap;
                }
                else
                {
                    model.Image = new Bitmap(1, 1);
                }
                
                if (model.EventType == EventType.RelativeToImage)
                {
                    model.MouseTriggerInfo.StartPoint = new Point(gameConfigView.RelativePosition.X, gameConfigView.RelativePosition.Y);
                }

                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if (btn.Equals(btnDelete))
            {
                var model = gameConfigView.CurrentTreeViewItem.DataContext<GameEventTriggerModel>();
                NotifyHelper.InvokeNotify(NotifyEventType.Delete, new DeleteEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = model,
                });
            }
            else if (btn.Equals(btnAddSameContent))
            {
                var item = gameConfigView.CopyCurrentItem();
                if (item == null)
                    return;
                NotifyHelper.InvokeNotify(NotifyEventType.Save, new SaveEventTriggerModelArgs()
                {
                    CurrentEventTriggerModel = item,
                });
            }
        }

        public override void CaptureImage(Bitmap bmp)
        {
            
        }
    }
}
