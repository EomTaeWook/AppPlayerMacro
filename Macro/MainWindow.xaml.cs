using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using Macro.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Utils;
using Utils.Document;
using Rect = Utils.Infrastructure.Rect;

namespace Macro
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            Init();
#if !DEBUG
            VersionCheck();
#endif
        }
        private void InitEvent()
        {
            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            btnStart.Click += Button_Click;
            btnStop.Click += Button_Click;
            btnSetting.Click += Button_Click;

            configView.SelectData += ConfigView_SelectData;
            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;
            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.EventTriggerOrderChanged += NotifyHelper_EventTriggerOrderChanged;
            Unloaded += MainWindow_Unloaded;
        }

        private void NotifyHelper_EventTriggerOrderChanged(EventTriggerOrderChangedEventArgs obj)
        {
            _taskQueue.Enqueue(() => 
            {
                return Delete(null);
            }).Finally((state) => {
                    Dispatcher.Invoke(() => 
                    {
                        Clear();
                    });
                }, this );
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
                captureImage.Background = new ImageBrush(capture.ToBitmapSource());
                var factor = NativeHelper.GetSystemDpi();
                _bitmap = new Bitmap(capture, (int)Math.Truncate(capture.Width * factor.X / ConstHelper.DefaultDPI), (int)Math.Truncate(capture.Height * factor.Y / ConstHelper.DefaultDPI));
            }
            WindowState = WindowState.Normal;
        }

        private void NotifyHelper_ConfigChanged(ConfigEventArgs e)
        {
            _config = e.Config;
            settingFlyout.IsOpen = !settingFlyout.IsOpen;
            Refresh();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _captureViews)
            {
                item.Close();
            }
            _captureViews.Clear();
        }
        private void ConfigView_SelectData(EventTriggerModel model)
        {
            if(model == null)
            {
                Clear();
            }
            else
            {
                var pair = comboProcess.Items.Cast<KeyValuePair<string, Process>>().Where(r => r.Key == model.ProcessInfo.ProcessName).FirstOrDefault();
                comboProcess.SelectedValue = pair.Value;
                btnDelete.Visibility = Visibility.Visible;
                _bitmap = model.Image;
                captureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnCapture))
            {
                Capture();
            }
            else if (btn.Equals(btnRefresh))
            {
                Refresh();
            }
            else if (btn.Equals(btnSave))
            {
                var model = configView.Model;
                model.Image = _bitmap;

                var process = comboProcess.SelectedValue as Process;

                model.ProcessInfo = new ProcessInfo()
                {
                    ProcessName = process ? .ProcessName,
                    Position = new Rect()
                };

                if (TryModelValidate(model, out Message error))
                {
                    var rect = new Rect();

                    NativeHelper.GetWindowRect(process.MainWindowHandle, ref rect);
                    model.ProcessInfo.Position = rect;
                    configView.InsertModel(model);

                    _taskQueue.Enqueue(Save, model).ContinueWith((task) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Clear();
                        });
                    });
                }
                else
                {
                    this.MessageShow("Error", DocumentHelper.Get(error));
                }
            }
            else if(btn.Equals(btnDelete))
            {
                var model = configView.Model;
                _taskQueue.Enqueue((o) =>
                {
                    var task = new TaskCompletionSource<Task>();
                    Dispatcher.Invoke(() =>
                    {
                        task.SetResult(Delete(o));
                        Clear();
                    });
                    return task.Task;
                }, model);
            }
            else if(btn.Equals(btnStart))
            {
                var buttons = this.FindChildren<Button>();
                foreach (var button in buttons)
                {
                    if (button.Equals(btnStart) || button.Equals(btnStop))
                        continue;
                    button.IsEnabled = false;
                }
                btnStop.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Collapsed;
                ProcessManager.Start();
            }
            else if(btn.Equals(btnStop))
            {
                this.ProgressbarShow(ProcessManager.Stop).ContinueWith((task) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var buttons = this.FindChildren<Button>();
                        foreach (var button in buttons)
                        {
                            if (button.Equals(btnStart) || button.Equals(btnStop))
                                continue;
                            button.IsEnabled = true;
                        }
                        btnStart.Visibility = Visibility.Visible;
                        btnStop.Visibility = Visibility.Collapsed;
                    });
                });
            }
            else if(btn.Equals(btnSetting))
            {
                settingFlyout.IsOpen = !settingFlyout.IsOpen;
            }
        }
    }
}
