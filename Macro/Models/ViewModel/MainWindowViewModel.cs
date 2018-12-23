using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Document;
using Macro.Extensions;
using System.ComponentModel;

namespace Macro.Models.ViewModel
{
    public class MainWindowViewModel
    {
        private readonly Language _language;
        private LabelDocument _document;
        public MainWindowViewModel(IConfig config)
        {
            _language = config.Language;
            _document = Singleton<LabelDocument>.Instance;
        }
        public string SaveConfig
        {
            get => _document.Get(_language, "SaveConfig");
        }
        public string ScreenCapture
        {
            get => _document.Get(_language, "ScreenCapture");
        }
        public string Config
        {
            get => _document.Get(_language, "Config");
        }
        public string CompareImage
        {
            get => _document.Get(_language, "CompareImage");
        }
        public string SelectProcess
        {
            get => _document.Get(_language, "SelectProcess");
        }
        public string Refresh
        {
            get => _document.Get(_language, "Refresh");
        }
    }
}
