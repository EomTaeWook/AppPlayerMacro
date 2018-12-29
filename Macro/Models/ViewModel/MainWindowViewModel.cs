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
        public string SaveConfig
        {
            get => DocumentHelper.Get(Label.SaveConfig);
        }
        public string ScreenCapture
        {
            get => DocumentHelper.Get(Label.ScreenCapture);
        }
        public string CompareImage
        {
            get => DocumentHelper.Get(Label.CompareImage);
        }
        public string SelectProcess
        {
            get => DocumentHelper.Get(Label.SelectProcess);
        }
        public string Refresh
        {
            get => DocumentHelper.Get(Label.Refresh);
        }
        public string Config
        {
            get => DocumentHelper.Get(Label.Config);
        }
        public string Save
        {
            get => DocumentHelper.Get(Label.Save);
        }
        public string Delete
        {
            get => DocumentHelper.Get(Label.Delete);
        }
        public string Start
        {
            get => DocumentHelper.Get(Label.Start);
        }
        public string Stop
        {
            get => DocumentHelper.Get(Label.Stop);
        }
    }
}
