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
        private readonly Language language;
        private LabelDocument _document;
        public MainWindowViewModel(IConfig config)
        {
            language = config.Language;
            _document = Singleton<LabelDocument>.Instance;
        }
        public string SelectProcess
        {
            get => _document.Get(language, "SelectProcess");
        }
        public string Refresh
        {
            get => _document.Get(language, "Refresh");
        }
    }
}
