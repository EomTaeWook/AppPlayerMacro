using KosherUtils.Log;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Utils;

namespace Macro
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                ex.Handled = true;
                Log.Warning(ex.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                var exception = ex.ExceptionObject as Exception;
                Log.Warning(exception);
            };
            AppDomain.CurrentDomain.FirstChanceException += (s, ex) =>
            {
#if DEBUG
                Log.Debug(ex.Exception.Message, 0, ex.Exception.TargetSite.DeclaringType.FullName);
#else
                Log.Warning(ex.Exception);
#endif
            };

            var exeList = e.Args.Where(r => Path.GetExtension(r).Equals(".exe")).ToArray();
            if(exeList.Count() > 0)
            {
                for (int i = 0; i < exeList.Length; ++i)
                {
                    TempFolderFileMove(exeList[i]);
                }
            }
            else
            {
                TempFolderFileMove(ConstHelper.DefaultPatcherName);
            }
            Init();
            base.OnStartup(e);
        }
    }
}