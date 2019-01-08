using Macro.Models;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Macro.View;
using Macro.Extensions;
using Utils.Document;
using Macro.Infrastructure;
using System.Threading.Tasks;
using Utils;
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
        }
        private void InitEvent()
        {
            btnCapture.Click += Button_Click;
            btnRefresh.Click += Button_Click;
            btnSave.Click += Button_Click;
            btnDelete.Click += Button_Click;
            btnStart.Click += Button_Click;
            btnStop.Click += Button_Click;

            configControl.SelectData += ConfigControl_SelectData;

            Unloaded += MainWindow_Unloaded;
        }
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in _captureViews)
            {
                item.DataBinding -= CaptureView_DataBinding;
                item.Close();
            }
            _captureViews.Clear();
        }
        private void ConfigControl_SelectData(EventTriggerModel model)
        {
            if(model == null)
            {
                Clear();
            }
            else
            {
                combo_process.SelectedValue = model.ProcessInfo.ProcessName;
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
                var model = configControl.Model;
                model.Image = _bitmap;

                var process = combo_process.SelectedValue as Process;

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

                    _taskQueue.Enqueue(Save, model).ContinueWith((task) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Clear();
                        });                        
                    }).Finally(r => ((ConfigEventView)r).InsertModel(model), configControl);
                }
                else
                {
                    this.MessageShow("Error", DocumentHelper.Get(error));
                }
            }
            else if(btn.Equals(btnDelete))
            {
                var model = configControl.Model;
                _taskQueue.Enqueue((o) =>
                {
                    configControl.RemoveModel(model);
                    return Task.CompletedTask;
                }, configControl)
                .ContinueWith((task) =>
                {
                    if (task.IsCompleted)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Delete(model);
                            Clear();
                        });
                    }
                });
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
                ProcessManager.Stop().ContinueWith((task) => 
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
        }
    }
}
