using Kosher.Framework;
using Kosher.Log;
using Macro.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Net;
using System.Windows;
using Utils;
using System.Collections.Generic;
using System.Windows.Interop;

namespace Macro.Infrastructure.Manager
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        public static MessageDialogResult ShowMessageDialog(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return ApplicationManager.Instance._mainWindow.ShowModalMessageExternal(title, message, style, new MetroDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Inverted,
            });
        }
        public static void ShowProgressbar()
        {
            ApplicationManager.Instance._mainWindow.Dispatcher.Invoke(() =>
            {
                ApplicationManager.Instance._progress.Width = ApplicationManager.Instance._mainWindow.ActualWidth;
                ApplicationManager.Instance._progress.Height = ApplicationManager.Instance._mainWindow.ActualHeight;

                ApplicationManager.Instance._progress.Left = ApplicationManager.Instance._mainWindow.Left;
                ApplicationManager.Instance._progress.Top = ApplicationManager.Instance._mainWindow.Top;
                ApplicationManager.Instance._progress.Show();
            });
        }
        public static void HideProgressbar()
        {
            ApplicationManager.Instance._progress.Hide();
        }

        private ProgressView _progress;

        private MetroWindow _mainWindow;

        private ChildWindow _drawWindow = new ChildWindow();

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
            foreach(var item in _captureViews)
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
            foreach(var item in _mousePointViews)
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
