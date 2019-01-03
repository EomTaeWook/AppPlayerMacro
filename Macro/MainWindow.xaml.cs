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
        }
        
        private void ConfigControl_SelectData(ConfigEventModel model)
        {
            if(model == null)
            {
                Clear();
            }
            else
            {
                combo_process.SelectedValue = model.ProcessName;
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
                _processes = Process.GetProcesses().ToList();
                combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();
            }
            else if (btn.Equals(btnSave))
            {
                var model = configControl.Model;
                model.Image = _bitmap;
                model.ProcessName = combo_process.SelectedValue as string;
                if (TryModelValidate(model, out Message error))
                {
                    _taskQueue.Enqueue(Save, model).ContinueWith((task) =>
                    {
                        this.Dispatcher.Invoke(() =>
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
                        this.Dispatcher.Invoke(() =>
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
                ProcessManager.Stop().Wait();

                var buttons = this.FindChildren<Button>();
                foreach (var button in buttons)
                {
                    if (button.Equals(btnStart) || button.Equals(btnStop))
                        continue;
                    button.IsEnabled = true;
                }
                btnStart.Visibility = Visibility.Visible;
                btnStop.Visibility = Visibility.Collapsed;
            }
        }
    }
}
