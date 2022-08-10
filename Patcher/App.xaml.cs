using KosherUtils.Framework;
using Patcher.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Utils;
using Utils.Document;
using Utils.Extensions;
using ConstHelper = Patcher.Infrastructure.ConstHelper;
using Version = Utils.Infrastructure.Version;

namespace Patcher
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private readonly ConcurrentDictionary<string, Assembly> _assembies;
        public App()
        {
            _assembies = new ConcurrentDictionary<string, Assembly>();
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(r => r.EndsWith(".dll"));
            foreach (var resource in resources)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    if (stream != null)
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        var assembly = Assembly.Load(buffer);
                        _assembies.GetOrAdd(assembly.FullName, assembly);
                    }
                }
            }
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Trace.WriteLine(args.Name);
            if(_assembies.TryGetValue(args.Name, out Assembly assembly))
            {
                return assembly;
            }
            return null;
        }        
        protected override void OnStartup(StartupEventArgs e)
        {
            var processes = Process.GetProcessesByName("Macro");
            foreach (var process in processes)
            {
                process.Kill();
            }
#if !DEBUG
            if(VersionValidate(e.Args) == false)
            {
                Current.Shutdown();
            }
            var currentVersion = Version.MakeVersion(e.Args[0]);
            var patchVersion = Version.MakeVersion(e.Args[1]);
#else
            var currentVersion = Version.MakeVersion("2.4.0");
            var patchVersion = Version.MakeVersion("9.6.0");
#endif

            if(currentVersion >= patchVersion)
            {
                Current.Shutdown();
            }

            ServiceProviderManager.AddService("CurrentVersion", currentVersion);
            ServiceProviderManager.AddService("PatchVersion", patchVersion);
            DependenciesResolved();
            InitDirectory();
            InitTemplate();
            

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void InitDirectory()
        {
            try
            {
                if(Directory.Exists(ConstHelper.TempBackupPath))
                {
                    Directory.Delete(ConstHelper.TempBackupPath, true);
                }
                if(Directory.Exists(ConstHelper.TempPath))
                {
                    Directory.Delete(ConstHelper.TempPath, true);
                }
                Directory.CreateDirectory(ConstHelper.TempPath);
                Directory.CreateDirectory(ConstHelper.TempBackupPath);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private bool VersionValidate(string [] args)
        {
            if (args.Count() != 2)
            {
                return false;
            }
            var currentVersion = args[0].Split('.');

            var nextVersion = args[1].Split('.');

            if (currentVersion.Count() != nextVersion.Count() || currentVersion.Count() != 3 || nextVersion.Count() != 3)
            {
                return false;
            }
               
            return true;
        }

        private void DependenciesResolved()
        {
            var path = Environment.CurrentDirectory + $@"\{Utils.ConstHelper.DefaultConfigFile}";

            if (File.Exists(path))
            {
                var config = JsonHelper.DeserializeObject<dynamic>(File.ReadAllText(path));

                if (Enum.TryParse(config["Language"].ToString(), true, result: out Language @enum))
                {
                    ServiceProviderManager.AddRefService("Language", @enum);
                }
            }
            else
            {
                ServiceProviderManager.AddRefService("Language", Language.Kor);
            }

            ServiceProviderManager.AddService("PatchUrl", Utils.ConstHelper.PatchV3Url.ToString());
            ServiceProviderManager.AddService("Label", Singleton<DocumentTemplate<Label>>.Instance);
            ServiceProviderManager.AddService("Message", Singleton<DocumentTemplate<Message>>.Instance);

        }
        private void InitTemplate()
        {
            var path = Utils.ConstHelper.DefaultDatasFilePath;
#if DEBUG
            path = $@"..\..\..\..\Datas\";
#endif
            ServiceProviderManager.GetService<DocumentTemplate<Label>>("Label").Init(path);
            ServiceProviderManager.GetService<DocumentTemplate<Message>>("Message").Init(path);
            
        }
    }
}
