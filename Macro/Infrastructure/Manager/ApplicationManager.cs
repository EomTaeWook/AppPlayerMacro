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
            return ApplicationManager.Instance._metroWindow.ShowModalMessageExternal(title, message, style, new MetroDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Inverted,
            });
        }
        public static void ShowProgressbar()
        {
            ApplicationManager.Instance._metroWindow.Dispatcher.Invoke(() =>
            {
                ApplicationManager.Instance._progress.Show();
            });
        }
        public static void HideProgressbar()
        {
            ApplicationManager.Instance._progress.Hide();
        }

        private ProgressView _progress;

        private MetroWindow _metroWindow;

        private readonly List<CaptureView> _captureViews = new List<CaptureView>();
        private readonly List<MousePositionView> _mousePointViews = new List<MousePositionView>();
        private IntPtr _mainWindowHandle;

        public ApplicationManager()
        {
            _metroWindow = Application.Current.MainWindow as MetroWindow;

            _progress = new ProgressView
            {
                Owner = _metroWindow,
                Left = _metroWindow.Left / 2,
                Width = _metroWindow.Width / 2,
                Top = _metroWindow.Top / 2,
                Height = _metroWindow.Height / 2
            };

            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
                _mousePointViews.Add(new MousePositionView(item));
            }
            
        }

        public IntPtr GetMainWindowHandle()
        {
            return _mainWindowHandle;
        }
        public void Init()
        {
            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
            _mainWindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
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

        public void ShowCaptureView()
        {
            foreach (var item in _captureViews)
            {
                item.ShowActivate();
            }
        }

        public void CloseCaptureView()
        {
            foreach(var item in _captureViews)
            {
                item.Hide();
            }
        }

        public void Dispose()
        {
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
