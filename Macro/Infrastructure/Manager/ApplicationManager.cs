using Dignus.Framework;
using Dignus.Log;
using Macro.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Interop;
using Utils;

namespace Macro.Infrastructure.Manager
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        public static MessageDialogResult ShowMessageDialog(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return Instance._mainWindow.ShowModalMessageExternal(title, message, style, new MetroDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Inverted,
            });
        }
        public static void ShowProgressbar()
        {
            Instance._mainWindow.Dispatcher.Invoke(() =>
            {
                Instance._progress.Width = Instance._mainWindow.ActualWidth;
                Instance._progress.Height = Instance._mainWindow.ActualHeight;

                Instance._progress.Left = Instance._mainWindow.Left;
                Instance._progress.Top = Instance._mainWindow.Top;
                Instance._progress.Show();
            });
        }
        public static void HideProgressbar()
        {
            Instance._progress.Hide();
        }

        private readonly ProgressView _progress;

        private readonly MetroWindow _mainWindow;

        private readonly ChildWindow _drawWindow = new ChildWindow();

        private readonly List<CaptureView> _captureViews = new List<CaptureView>();
        private readonly List<MousePositionView> _mousePointViews = new List<MousePositionView>();
        private IntPtr _drawWindowHandle;

        public ApplicationManager()
        {
            _mainWindow = Application.Current.MainWindow as MetroWindow;

            _progress = new ProgressView
            {
                Owner = _mainWindow,
                RenderSize = _mainWindow.RenderSize
            };

            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
                _mousePointViews.Add(new MousePositionView(item));
            }
        }

        public Window GetDrawWindow()
        {
            return _drawWindow;
        }
        public IntPtr GetDrawWindowHandle()
        {
            return _drawWindowHandle;
        }
        public void Init()
        {
            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
            _drawWindow.Opacity = 0;
            _drawWindow.Show();
#if DEBUG
            //_drawWindow.Opacity = 1;
#endif
            _drawWindowHandle = new WindowInteropHelper(_drawWindow).Handle;
            SchedulerManager.Instance.Start();
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        public void ShowMousePointView()
        {
            foreach (var item in _mousePointViews)
            {
                item.ShowActivate();
            }
        }
        public void CloseMousePointView()
        {
            foreach (var item in _mousePointViews)
            {
                item.Hide();
            }
        }

        public void ShowImageCaptureView()
        {
            foreach (var item in _captureViews)
            {
                item.ShowActivate(CaptureModeType.ImageCapture);
            }
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        public void ShowSetROIView()
        {
            foreach (var item in _captureViews)
            {
                item.ShowActivate(CaptureModeType.ROICapture);
            }
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        public void CloseCaptureView()
        {
            foreach (var item in _captureViews)
            {
                item.Hide();
            }
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        public void Dispose()
        {
            _drawWindow.Close();
            SchedulerManager.Instance.Stop();
            _progress.Close();
            foreach (var item in _captureViews)
            {
                item.Close();
            }
            foreach (var item in _mousePointViews)
            {
                item.Close();
            }
        }
        public VersionNote GetLatestVersion()
        {
            VersionNote versionNote = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(ConstHelper.VersionUrl);
                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        versionNote = JsonHelper.DeserializeObject<VersionNote>(stream.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            return versionNote;
        }
    }
}
