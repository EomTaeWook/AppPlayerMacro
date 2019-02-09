using Newtonsoft.Json.Linq;
using Patcher.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Patcher
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class App : Application
    {
        private AppDomain _utilAppDomain = AppDomain.CreateDomain("UtilAppDomain");
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
                string resourceName = resources.First();
                using (Stream stream = assemblies.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        return Assembly.Load(buffer);
                    }
                }
            }
            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var listener = new Listener();
            var obj = (Loader)_utilAppDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(Loader).FullName);
            obj.Init(listener);
            var caches = obj.GetCaches();
            foreach (var cache in caches)
            {
                ObjectCache.SetValue(cache.Key, cache.Value);
            }
            AppDomain.Unload(_utilAppDomain);
            base.OnStartup(e);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.ManifestModule.Name.Equals("Utils.dll"))
            {
                Trace.WriteLine($">>>>{args.LoadedAssembly.ManifestModule.Name}<<<<");
            }
            else
            {
                //Trace.WriteLine(args.LoadedAssembly.ManifestModule.Name);
            }
        }
        [Serializable]
        public class Loader : MarshalByRefObject
        {
            Listener _listener;
            public void Init(Listener listener)
            {
                _listener = listener;
                var name = AppDomain.CurrentDomain.FriendlyName;
                var path = Environment.CurrentDirectory + $@"\{ConstHelper.DefaultConfigFile}";

                if (File.Exists(path))
                {
                    var configObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));
                    var pair = configObj.First;
                    while(pair != null)
                    {
                        var value = pair.GetType().GetProperty("Value").GetValue(pair);
                        if (Enum.TryParse(value.ToString(), true, out Language language))
                        {
                            listener.Insert("language", value.ToString());
                            break;
                        }
                        pair = pair.Next;
                    }
                }
                InitTemplate();
            }
            public void InitTemplate()
            {
                var path = ConstHelper.DefaultDatasFile;
#if DEBUG
                path = $@"..\..\..\Datas\";
#endif
                var labelTemplate = new DocumentTemplate<Label>();
                labelTemplate.Init(path);
                var messageTemplate = new DocumentTemplate<Message>();
                messageTemplate.Init(path);

                _listener.Insert("PatchUrl", ConstHelper.PatchUrl.ToString());

                if (Enum.TryParse(ObjectCache.GetValue("language").ToString(), out Language language))
                {
                    _listener.Insert("SearchPatchList", labelTemplate[Label.SearchPatchList, language]);
                    _listener.Insert("Cancel", labelTemplate[Label.Cancel, language]);
                    _listener.Insert("Download", labelTemplate[Label.Download, language]);
                    _listener.Insert("Patching", labelTemplate[Label.Patching, language]);
                    _listener.Insert("Rollback", labelTemplate[Label.Rollback, language]);

                    _listener.Insert("CancelPatch", messageTemplate[Message.CancelPatch, language]);
                }
            }
            public KeyValuePair<string, object>[] GetCaches()
            {
                return _listener.GetCaches();
            }
        }

        [Serializable]
        public class Listener
        {
            public void Insert(string key, object value)
            {
                ObjectCache.SetValue(key, value);
            }
            public KeyValuePair<string, object>[] GetCaches()
            {
                return ObjectCache.GetCaches();
            }
        }
    }
}
