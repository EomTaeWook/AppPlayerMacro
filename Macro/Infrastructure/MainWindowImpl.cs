using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models;
using Macro.View;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Unity;
using Utils;
using Utils.Infrastructure;
using Message = Utils.Document.Message;

namespace Macro
{
    public partial class MainWindow : MetroWindow
    {
        private string _path;
        private void Init()
        {
            _processes = Process.GetProcesses().ToList();
            combo_process.ItemsSource = _processes.OrderBy(r => r.ProcessName).Select(r => r.ProcessName).ToList();
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
            if (dpiTransform.CanFreeze)
                dpiTransform.Freeze();
            this.LayoutTransform = dpiTransform;

            _path = _config.SavePath;
            if (string.IsNullOrEmpty(_path))
                _path = ConstHelper.DefaultSavePath;
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            _path = $"{_path}{ConstHelper.DefaultSaveFile}";   
        }

        private bool TryModelValidate(ConfigEventModel model, out Message message)
        {
            message = Message.Success;
            model.KeyboardCmd = model.KeyboardCmd.Replace(" ", "");
            if (model.Image == null)
            {
                message = Message.FailedImageValidate;
                return false;
            }
            if (model.EventType == EventType.Mouse && model.MousePoint == null)
            {
                message = Message.FailedMouseCoordinatesValidate;
                return false;
            }

            if (string.IsNullOrEmpty(model.KeyboardCmd) && model.EventType == EventType.Keyboard)
            {
                message = Message.FailedKeyboardCommandValidate;
                return false;
            }
            if (string.IsNullOrEmpty(model.ProcessName))
            {
                message = Message.FailedProcessValidate;
                return false;
            }
            return true;
        }
        private void Capture()
        {
            Clear();
            var capture = new CaptureView();
            WindowState = System.Windows.WindowState.Minimized;
            capture.ShowDialog();
            if (capture.CaptureImage != null)
            {
                _bitmap = capture.CaptureImage;
                captureImage.Background = new ImageBrush(_bitmap.ToBitmapSource());
            }
            this.WindowState = System.Windows.WindowState.Normal;
        }
        private void Clear()
        {
            btnDelete.Visibility = System.Windows.Visibility.Collapsed;
            _bitmap = null;
            captureImage.Background = System.Windows.Media.Brushes.White;
            configControl.Clear();
        }
        private Task Delete(object m)
        {
            var model = m as ConfigEventModel;
            if (File.Exists(_path))
            {
                File.Delete(_path);
                using (var fs = new FileStream(_path, FileMode.CreateNew))
                {
                    foreach (var data in (configControl.DataContext as Models.ViewModel.ConfigEventViewModel).ConfigSaves)
                    {
                        var bytes = ObjectExtensions.SerializeObject(data);
                        fs.Write(bytes, 0, bytes.Count());
                    }
                    fs.Close();
                }
            }
            return Task.CompletedTask;
        }
        private Task Save(object m)
        {
            var model = m as ConfigEventModel;
            model.Index = _index++;
            using (var fs = new FileStream(_path, FileMode.Append))
            {
                var bytes = ObjectExtensions.SerializeObject(model);
                fs.Write(bytes, 0, bytes.Count());
                fs.Close();
            }
            return Task.CompletedTask;
        }
        private void SaveLoad()
        {
            if (File.Exists(_path))
            {
                var models = ObjectExtensions.DeserializeObject(File.ReadAllBytes(_path));
                _index = models.LastOrDefault()?.Index ?? 0;
                foreach (var model in models)
                {
                    configControl.InsertModel(model);
                }
            }
        }
        private Task OnProcessCallback()
        {
            var result = new TaskCompletionSource<Task>();
            Dispatcher.Invoke(() =>
            {
                var configSaves = (configControl.DataContext as Models.ViewModel.ConfigEventViewModel).ConfigSaves;
                foreach (var save in configSaves)
                {
                    var processes = _processes.Where(r => r.ProcessName.Equals(save.ProcessName)).ToList();
                    foreach (var process in processes)
                    {
                        if (CaptureHelper.ProcessCapture(process, out Bitmap bmp))
                        {
                            var similarity = OpenCVHelper.Search(bmp, save.Image);
                            LogHelper.Debug($"similarity : {similarity}");
                            if (similarity >= _config.Similarity)
                            {
                                if(save.EventType == EventType.Mouse)
                                {

                                }
                                else if(save.EventType == EventType.Keyboard)
                                {
                                    InputManager.ModifiedKeyStroke(KeyCode.LWIN, KeyCode.KEY_R);
                                    System.Threading.Thread.Sleep(1000);
                                    //var hWndActive = NativeHelper.GetForegroundWindow();
                                    //var commands = save.KeyboardCmd.Split('+');
                                    //var modifiedKey = commands.Where(r =>
                                    //{
                                    //    var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), $"{r}", true);
                                    //    return keyCode.IsExtendedKey();
                                    //}).Select(r =>
                                    //{
                                    //    var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), $"{r}", true);
                                    //    return keyCode;
                                    //});

                                    //var keys = commands.Where(r =>
                                    //{
                                    //    var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), r, true);
                                    //    return !keyCode.IsExtendedKey();
                                    //}).Select(r =>
                                    //{
                                    //    var keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), r, true);
                                    //    return keyCode;
                                    //});
                                    //NativeHelper.SetForegroundWindow(process.MainWindowHandle);
                                    //InputManager.ModifiedKeyStroke(modifiedKey, keys);
                                    //NativeHelper.SetForegroundWindow(hWndActive);
                                }
                            }
                        }
                    }
                }
                result.SetResult(Task.CompletedTask);
            });
            return result.Task;
        }
    }
}
