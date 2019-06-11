using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Manager;
using Macro.Models;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Utils;
using Utils.Infrastructure;

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
            VersionCheck();
        }
        private void InitEvent()
        {
            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            btnAddSameContent.Click += Button_Click;
            btnStart.Click += Button_Click;
            btnStop.Click += Button_Click;
            btnSetting.Click += Button_Click;
            btnGithub.Click += Button_Click;
            checkFix.Checked += CheckFix_Checked;
            checkFix.Unchecked += CheckFix_Checked;
            comboProcess.SelectionChanged += ComboProcess_SelectionChanged;

            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;
            NotifyHelper.ScreenCaptureDataBind += NotifyHelper_ScreenCaptureDataBind;
            NotifyHelper.TreeItemOrderChanged += NotifyHelper_TreeItemOrderChanged;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;
            NotifyHelper.EventTriggerOrderChanged += NotifyHelper_EventTriggerOrderChanged;

            Unloaded += MainWindow_Unloaded;
        }

        private void NotifyHelper_EventTriggerOrderChanged(EventTriggerOrderChangedEventArgs obj)
        {
            _taskQueue.Enqueue(() =>
            {
                return SaveFile();
            });
        }

        private void ComboProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFix_Checked(checkFix, null);
        }
        private void CheckFix_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(checkFix))
            {
                if (checkFix.IsChecked.HasValue)
                {
                    if(checkFix.IsChecked.Value)
                    {
                        if(comboProcess.SelectedItem is KeyValuePair<string, Process> item)
                        {
                            _fixProcess = new KeyValuePair<string, Process>(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        _fixProcess = null;
                    }                        
                }
                else
                {
                    _fixProcess = null;
                }
            }
        }

        private void NotifyHelper_SelectTreeViewChanged(SelctTreeViewItemChangedEventArgs e)
        {
            if (e.TreeViewItem == null)
            {
                Clear();
            }
            else
            {
                var model = e.TreeViewItem.DataContext<EventTriggerModel>();
                if(_fixProcess == null)
                {
                    var pair = comboProcess.Items.Cast<KeyValuePair<string, Process>>().Where(r => r.Key == model.ProcessInfo.ProcessName).FirstOrDefault();
                    comboProcess.SelectedValue = pair.Value;
                }
                else
                    comboProcess.SelectedValue = _fixProcess.Value.Value;

                btnDelete.Visibility = Visibility.Visible;
                btnAddSameContent.Visibility = Visibility.Visible;

                _bitmap = model.Image;
                captureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
        }
        private void NotifyHelper_TreeItemOrderChanged(EventTriggerOrderChangedEventArgs e)
        {
            _taskQueue.Enqueue(() => 
            {
                return Delete();
            }).Finally(state => 
            {
                Dispatcher.Invoke(() => 
                {
                    Clear();
                });
            }, this);
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
                _bitmap = new Bitmap(capture, capture.Width, capture.Height);
            }
            WindowState = WindowState.Normal;
        }

        private void NotifyHelper_ConfigChanged(ConfigEventArgs e)
        {
            this.ProgressbarShow(() =>
            {
                _config = e.Config;
                (configView.DataContext as Models.ViewModel.ConfigEventViewModel).TriggerSaves.Clear();
                Refresh();
                SaveFileLoad(null);
                settingFlyout.IsOpen = !settingFlyout.IsOpen;
                return Task.CompletedTask;
            });
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
                Capture();
            }
            else if (btn.Equals(btnRefresh))
            {
                Refresh();
            }
            else if (btn.Equals(btnSave))
            {
                Save();
            }
            else if(btn.Equals(btnDelete))
            {
                _taskQueue.Enqueue((o) =>
                {
                    var task = new TaskCompletionSource<Task>();
                    Dispatcher.Invoke(() =>
                    {
                        task.SetResult(Delete());
                        Clear();
                    });
                    return task.Task;
                }, null);
            }
            else if(btn.Equals(btnAddSameContent))
            {
                configView.CopyCurrentItem();
                Save();
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

                var job = TaskBuilder.Build(()=> { }, out CancellationToken token);

                //var task = ProcessManager.Start();
                //if(task.IsFaulted)
                //{
                //    ProcessManager.Stop();
                //    ProcessManager.Start();
                //}
            }
            else if(btn.Equals(btnStop))
            {
                //this.ProgressbarShow(ProcessManager.Stop).ContinueWith(task =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        var buttons = this.FindChildren<Button>();
                //        foreach (var button in buttons)
                //        {
                //            if (button.Equals(btnStart) || button.Equals(btnStop))
                //                continue;
                //            button.IsEnabled = true;
                //        }
                //        btnStart.Visibility = Visibility.Visible;
                //        btnStop.Visibility = Visibility.Collapsed;
                //        configView.Clear();
                //    });
                //});
            }
            else if(btn.Equals(btnSetting))
            {
                settingFlyout.IsOpen = !settingFlyout.IsOpen;
            }
            else if(btn.Equals(btnGithub))
            {
                Process.Start(ConstHelper.HelpUrl);
            }
        }
    }
}
