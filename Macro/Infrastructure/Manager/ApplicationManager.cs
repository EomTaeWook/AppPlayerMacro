using KosherUtils.Framework;
using KosherUtils.Log;
using Macro.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Net;
using System.Windows;
using Utils;
using System.Collections.Generic;

namespace Macro.Infrastructure.Manager
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        public static MessageDialogResult ShowMessageDialog(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
            return ApplicationManager.Instance.metroWindow.ShowModalMessageExternal(title, message, style, new MetroDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Inverted,
            });
        }
        public static void ShowProgressbar()
        {
            ApplicationManager.Instance.metroWindow.Dispatcher.Invoke(() =>
            {
                ApplicationManager.Instance.progress.Show();
            });
        }
        public static void HideProgressbar()
        {
            ApplicationManager.Instance.progress.Hide();
        }

        private ProgressView progress;

        private MetroWindow metroWindow;

        private readonly List<CaptureView> _captureViews = new List<CaptureView>();
        private readonly List<MousePositionView> _mousePointViews = new List<MousePositionView>();

        public ApplicationManager()
        {
            metroWindow = Application.Current.MainWindow as MetroWindow;

            progress = new ProgressView
            {
                Owner = metroWindow,
                Left = metroWindow.Left / 2,
                Width = metroWindow.Width / 2,
                Top = metroWindow.Top / 2,
                Height = metroWindow.Height / 2
            };

            foreach (var item in DisplayHelper.MonitorInfo())
            {
                _captureViews.Add(new CaptureView(item));
                _mousePointViews.Add(new MousePositionView(item));
            }
            
        }
        public void Init()
        {
            Application.Current.MainWindow.Unloaded += MainWindow_Unloaded;
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
            progress.Close();
            foreach (var item in _captureViews)
            {
                item.Close();
            }
            foreach(var item in _mousePointViews)
            {
                item.Close();
            }
        }

        public Version GetLatestVersion()
        {
            Version version = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(ConstHelper.VersionUrl);
                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        version = JsonHelper.DeserializeObject<Version>(stream.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            return version;
        }
    }
}
