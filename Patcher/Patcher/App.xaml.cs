using Patcher.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Utils;
using Utils.Document;
using Utils.Extensions;

namespace Patcher
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#if DEBUG
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
#endif
        }
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblies = Assembly.GetExecutingAssembly();
            var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            var resources = assemblies.GetManifestResourceNames().Where(s => s.EndsWith(name));
            if (resources.Count() > 0)
            {
                var resourceName = resources.First();
                using (Stream stream = assemblies.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        return Assembly.Load(buffer);
                    }
                }
            }
            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            if(e.Args.Count() != 2)
                Current.Shutdown();
            if(!VersionValidate(e.Args[0].Split('.'), e.Args[1].Split('.'), out int compare))
                Current.Shutdown();
            ObjectCache.SetValue("Version", compare);
#else
            ObjectCache.SetValue("Version", 1);
#endif
            foreach(var item in Dependency.List)
            {
                if(File.Exists(item))
                    File.SetAttributes(item, FileAttributes.Hidden);
            }

            Init();
            InitTemplate();
            LogHelper.Init();

            foreach (var item in Dependency.List)
            {
                if (File.Exists(item))
                    File.SetAttributes(item, FileAttributes.Normal);
            }

            base.OnStartup(e);
        }

        private bool VersionValidate(string[] current, string[] next, out int compare)
        {
            compare = 0;

            if (current.Count() != next.Count() || current.Count() != 3 || next.Count() != 3)
                return false;

            var currentVersion = new Infrastructure.Version()
            {
                Major = Convert.ToInt32(current[0]),
                Minor = Convert.ToInt32(current[1]),
                Build = Convert.ToInt32(current[2])
            };
            var nextVersion = new Infrastructure.Version()
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
                Trace.WriteLine($">>>>{args.LoadedAssembly.ManifestModule.Name}<<<<");
            }
        }
        private void Init()
        {
            var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";

            if (File.Exists(path))
            {
                var configObj = JsonHelper.DeserializeObject<dynamic>(File.ReadAllText(path));
                var pair = configObj.First;
                while (pair != null)
                {
                    var value = pair.GetType().GetProperty("Value").GetValue(pair);
                    if (Enum.TryParse(value.ToString(), true, out Language language))
                    {
                        ObjectCache.SetValue("language", value.ToString());
                        break;
                    }
                    pair = pair.Next;
                }
            }
        }
        private void InitTemplate()
        {
            var path = ConstHelper.DefaultDatasFile;
#if DEBUG
            path = $@"..\..\..\Datas\";
#endif
            Singleton<DocumentTemplate<Label>>.Instance.Init(ConstHelper.DefaultDatasFile);
            Singleton<DocumentTemplate<Message>>.Instance.Init(ConstHelper.DefaultDatasFile);

            ObjectCache.SetValue("PatchUrl", ConstHelper.PatchUrl.ToString());
            
            if (Enum.TryParse(ObjectCache.GetValue("language").ToString(), out Language language))
            {
                ObjectCache.SetValue("SearchPatchList", DocumentExtensions.Get(Label.SearchPatchList, language));
                ObjectCache.SetValue("Cancel", DocumentExtensions.Get(Label.Cancel, language));
                ObjectCache.SetValue("Download", DocumentExtensions.Get(Label.Download, language));
                ObjectCache.SetValue("Patching", DocumentExtensions.Get(Label.Patching, language));
                ObjectCache.SetValue("Rollback", DocumentExtensions.Get(Label.Rollback, language));

                ObjectCache.SetValue("CancelPatch", DocumentExtensions.Get(Message.CancelPatch, language));
            }
        }
    }
}
