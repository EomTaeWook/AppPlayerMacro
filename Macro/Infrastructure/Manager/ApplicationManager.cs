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

namespace Macro.Infrastructure.Manager
{
    public class ApplicationManager : Singleton<ApplicationManager>
    {
        public static MessageDialogResult MessageShow(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
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
