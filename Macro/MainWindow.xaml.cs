using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
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
            btnRefresh.Click += Button_Click;            
            btnStart.Click += Button_Click;
            btnStop.Click += Button_Click;
            btnSetting.Click += Button_Click;
            btnGithub.Click += Button_Click;
            checkFix.Checked += CheckFix_Checked;
            checkFix.Unchecked += CheckFix_Checked;
            comboProcess.SelectionChanged += ComboProcess_SelectionChanged;

            NotifyHelper.ConfigChanged += NotifyHelper_ConfigChanged;
            NotifyHelper.TreeItemOrderChanged += NotifyHelper_TreeItemOrderChanged;
            NotifyHelper.SelectTreeViewChanged += NotifyHelper_SelectTreeViewChanged;
            NotifyHelper.EventTriggerOrderChanged += NotifyHelper_EventTriggerOrderChanged;
            NotifyHelper.SaveEventTriggerModel += NotifyHelper_SaveEventTriggerModel;
        }

        private void NotifyHelper_SaveEventTriggerModel(SaveEventTriggerModelArgs obj)
        {
            if(Validate(obj.CurrentEventTriggerModel))
            {
                if (tab_content.SelectedContent is BaseContentView view)
                {
                    _taskQueue.Enqueue(() => 
                    {
                        ObjectExtensions.GetInstance<CacheDataManager>().MakeIndexTriggerModel(obj.CurrentEventTriggerModel);
                        return view.Save(_savePath + $"{ConstHelper.DefaultSaveFileName}");
                    }).ContinueWith(task => 
                    {
                        if(task.IsFaulted == false)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Clear();
                            });
                        }
                        else
                        {
                            this.MessageShow("Error", DocumentHelper.Get(Utils.Document.Message.FailedSaveFile));
                        }
                    });
                }
            }
        }
        private void NotifyHelper_TreeItemOrderChanged(EventTriggerOrderChangedEventArgs e)
        {
            if (tab_content.SelectedContent is BaseContentView view)
            {
                _taskQueue.Enqueue(() =>
                {
                    return view.Delete(_savePath + $"{ConstHelper.DefaultSaveFileName}");
                }).ContinueWith(task => 
                {
                    Dispatcher.Invoke(() =>
                    {
                        view.Clear();
                        Clear();
                    });
                }); ;
            }
        }
        private void NotifyHelper_EventTriggerOrderChanged(EventTriggerOrderChangedEventArgs obj)
        {
            if(tab_content.SelectedContent is BaseContentView view)
            {
                _taskQueue.Enqueue(() =>
                {
                    return view.Save(_savePath + $"{ConstHelper.DefaultSaveFileName}");
                });
            }
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
                if (_fixProcess == null)
                {
                    var pair = comboProcess.Items.Cast<KeyValuePair<string, Process>>().Where(r => r.Key == model.ProcessInfo.ProcessName).FirstOrDefault();
                    comboProcess.SelectedValue = pair.Value;
                }
                else
                    comboProcess.SelectedValue = _fixProcess.Value.Value;
            }
        }
        private void NotifyHelper_ConfigChanged(ConfigEventArgs e)
        {
            this.ProgressbarShow(() =>
            {
                _config = e.Config;
                //(configView.DataContext as Models.ViewModel.ConfigEventViewModel).TriggerSaves.Clear();
                Refresh();
                SaveFileLoad(_savePath);
                settingFlyout.IsOpen = !settingFlyout.IsOpen;
                return Task.CompletedTask;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Equals(btnRefresh))
            {
                Refresh();
            }
            else if (btn.Equals(btnStart))
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
                if (tokenSource == null)
                {
                    TaskBuilder.Build(ProcessStartAsync, out CancellationTokenSource cancellationTokenSource);
                    tokenSource = cancellationTokenSource;
                }
            }
            else if (btn.Equals(btnStop))
            {
                this.ProgressbarShow(() =>
                {
                    if (tokenSource != null)
                    {
                        tokenSource.Cancel();
                        tokenSource = null;
                    }
                    _taskQueue.Clear();
                    return Task.CompletedTask;
                }).ContinueWith(task =>
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
            else if (btn.Equals(btnSetting))
            {
                settingFlyout.IsOpen = !settingFlyout.IsOpen;
            }
            else if (btn.Equals(btnGithub))
            {
                Process.Start(ConstHelper.HelpUrl);
            }
            //if (btn.Equals(btnCapture))
            //{
            //    Capture();
            //}
            //else if(btn.Equals(btnDelete))
            //{
            //    _taskQueue.Enqueue((o) =>
            //    {
            //        var task = new TaskCompletionSource<Task>();
            //        Dispatcher.Invoke(() =>
            //        {
            //            task.SetResult(Delete(_savePath));
            //            Clear();
            //        });
            //        return task.Task;
            //    }, null);
            //}
            //else if(btn.Equals(btnAddSameContent))
            //{
            //    configView.CopyCurrentItem();
            //    Save();
            //}
        }
    }
}
