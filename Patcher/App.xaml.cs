using Patcher.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Utils;
using Utils.Document;
using Utils.Extensions;
using ConstHelper = Patcher.Infrastructure.ConstHelper;

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
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
            if(e.Args.Count() != 2)
                Current.Shutdown();
            if(!VersionValidate(e.Args[0].Split('.'), e.Args[1].Split('.'), out int compare, out Infrastructure.Version nextVersion))
                Current.Shutdown();
            ObjectCache.SetValue("Version", compare);
            ObjectCache.SetValue("PathchVersion", nextVersion);
#else
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            ObjectCache.SetValue("Version", 1);
            ObjectCache.SetValue("PathchVersion", new Infrastructure.Version(2, 4, 1));
#endif
            InitDirectory();
            Init();
            InitTemplate();

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            foreach (var item in Dependency.List)
            {
                if (File.Exists(item))
                    continue;
                File.Move($"{ConstHelper.TempBackupPath}{item}", item);
            }
            if(ObjectCache.GetValue("PatchMethod") is PatchMethod patchMethod)
            {
                if(patchMethod == PatchMethod.Exe && File.Exists(@".\Macro.exe"))
                {
                    Process.Start(@".\Macro.exe", $"{ObjectCache.GetValue("Patcher") ?? ""}");
                }
            }
            else
            {
                Process.Start(@".\Macro.exe", $"{ObjectCache.GetValue("Patcher") ?? ""}");
            }
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

            foreach (var item in Dependency.List)
            {
                if (File.Exists(item))
                {
                    if (File.Exists($"{ConstHelper.TempBackupPath}{item}"))
                        File.Delete($"{ConstHelper.TempBackupPath}{item}");

                    File.Move(item, $"{ConstHelper.TempBackupPath}{item}");
                }
            }
        }

        private bool VersionValidate(string[] current, string[] next, out int compare, out Infrastructure.Version nextVersion)
        {
            compare = 0;

            if (current.Count() != next.Count() || current.Count() != 3 || next.Count() != 3)
            {
                nextVersion = null;
                return false;
            }

            var currentVersion = new Infrastructure.Version()
            {
                Major = Convert.ToInt32(current[0]),
                Minor = Convert.ToInt32(current[1]),
                Build = Convert.ToInt32(current[2])
            };
            nextVersion = new Infrastructure.Version()
            {
                Major = Convert.ToInt32(next[0]),
                Minor = Convert.ToInt32(next[1]),
                Build = Convert.ToInt32(next[2]),
            };
            compare = nextVersion.CompareTo(currentVersion);

            return true;
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.ManifestModule.Name.Equals("Utils.dll"))
            {
                Trace.WriteLine($">>>>{args.LoadedAssembly.ManifestModule.Name} Loaded<<<<");
            }
        }
        private void Init()
        {
            var path = Environment.CurrentDirectory + $@"\{Utils.ConstHelper.DefaultConfigFile}";

            if (File.Exists(path))
            {
                var config = JsonHelper.DeserializeObject<dynamic>(File.ReadAllText(path));
                if (Enum.TryParse(config["Language"].ToString(), true, result: out Language _))
                {
                    ObjectCache.SetValue("language", config["Language"].ToString());
                }
            }
            else
            {
                ObjectCache.SetValue("language", Language.Kor.ToString());
            }
        }
        private void InitTemplate()
        {
            Singleton<DocumentTemplate<Label>>.Instance.Init(Utils.ConstHelper.DefaultDatasFilePath);
            Singleton<DocumentTemplate<Message>>.Instance.Init(Utils.ConstHelper.DefaultDatasFilePath);

            ObjectCache.SetValue("PatchUrl", Utils.ConstHelper.PatchV2Url.ToString());
            
            if (Enum.TryParse(ObjectCache.GetValue("language").ToString(), out Language language))
            {
                ObjectCache.SetValue("SearchPatchList", DocumentExtensions.Get(Label.SearchPatchList, language));
                ObjectCache.SetValue("Cancel", DocumentExtensions.Get(Label.Cancel, language));
                ObjectCache.SetValue("Download", DocumentExtensions.Get(Label.Download, language));                
                ObjectCache.SetValue("Patching", DocumentExtensions.Get(Label.Patching, language));
                ObjectCache.SetValue("Rollback", DocumentExtensions.Get(Label.Rollback, language));
                ObjectCache.SetValue("FailedPatch", DocumentExtensions.Get(Label.FailedPatch, language));

                ObjectCache.SetValue("CancelPatch", DocumentExtensions.Get(Message.CancelPatch, language));
                ObjectCache.SetValue("FailedPatchUpdate", DocumentExtensions.Get(Message.FailedPatchUpdate, language));
            }
        }
    }
}
