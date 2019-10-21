using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Infrastructure.Impl;
using Macro.Infrastructure.Manager;
using Macro.Models;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Utils;
using Utils.Document;
using Utils.Extensions;
using Utils.Infrastructure;
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
            NotifyHelper.DeleteEventTriggerModel += NotifyHelper_DeleteEventTriggerModel;
            
        }
        private void NotifyHelper_DeleteEventTriggerModel(DeleteEventTriggerModelArgs obj)
        {
            if (tab_content.SelectedContent is BaseContentView view)
            {
                var path = _viewMap[view.Tag.ToString()].SaveFilePath;
                _taskQueue.Enqueue(() =>
                {
                    return view.Delete(path);
                }).ContinueWith(task =>
                {
                    Clear();
                });
            }
        }

        private void NotifyHelper_SaveEventTriggerModel(SaveEventTriggerModelArgs obj)
        {
            if (tab_content.SelectedContent is BaseContentView baseView)
            {
                var viewObj = _viewMap[baseView.Tag.ToString()];

                var process = comboProcess.SelectedValue as Process;

                obj.CurrentEventTriggerModel.ProcessInfo.ProcessName = process?.ProcessName;

                if (viewObj.View.Validate(obj.CurrentEventTriggerModel, out Message error))
                {
                    var path = viewObj.SaveFilePath;

                    _taskQueue.Enqueue(() =>
                    {
                        Dispatcher.Invoke(() => 
                        {
                            if (obj.CurrentEventTriggerModel is GameEventTriggerModel)
                            {
                                var model = obj.CurrentEventTriggerModel as GameEventTriggerModel;
                                ObjectExtensions.GetInstance<CacheDataManager>().MakeIndexTriggerModel(model);
                            }
                            else if (obj.CurrentEventTriggerModel is EventTriggerModel)
                            {
                                var model = obj.CurrentEventTriggerModel as EventTriggerModel;
                                ObjectExtensions.GetInstance<CacheDataManager>().MakeIndexTriggerModel(model);
                                SettingProcessMonitorInfo(model, process);
                            }
                        });
                        return viewObj.View.Save(path);
                    }).ContinueWith(task =>
                    {
                        if (task.IsFaulted == false)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Clear();
                            });
                        }
                        else
                        {
                            this.MessageShow("Error", DocumentHelper.Get(Message.FailedSaveFile));
                        }
                    });
                }
                else
                {
                    this.MessageShow("Error", DocumentHelper.Get(error));
                }
            }
        }
        private void NotifyHelper_TreeItemOrderChanged(EventTriggerOrderChangedEventArgs e)
        {
            if (tab_content.SelectedContent is BaseContentView view)
            {
                var path = _viewMap[view.Tag.ToString()].SaveFilePath;
                _taskQueue.Enqueue(() =>
                {
                    return view.Delete(path);
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
                var path = _viewMap[view.Tag.ToString()].SaveFilePath;
                _taskQueue.Enqueue(() =>
                {
                    return view.Save(path);
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
            if(e.TreeViewItem == null)
            {
                return;
            }

            var model = e.TreeViewItem.DataContext<IBaseEventTriggerModel>();

            if (model == null)
            {
                Clear();
                return;
            }

            if (_fixProcess == null)
            {
                var pair = comboProcess.Items.Cast<KeyValuePair<string, Process>>().Where(r => r.Key == model.ProcessInfo.ProcessName).FirstOrDefault();
                comboProcess.SelectedValue = pair.Value;
            }
            else
                comboProcess.SelectedValue = _fixProcess.Value.Value;
        }
        private void NotifyHelper_ConfigChanged(ConfigEventArgs e)
        {
            this.ProgressbarShow(() =>
            {
                _config = e.Config;
                Refresh();
                foreach(var item in _viewMap)
                {
                    item.Value.View.Clear();
                    _taskQueue.Enqueue(item.Value.View.Load, item.Value);
                }
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
                tab_content.IsEnabled = false;
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
                    tab_content.IsEnabled = true;
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
        }
    }
}
