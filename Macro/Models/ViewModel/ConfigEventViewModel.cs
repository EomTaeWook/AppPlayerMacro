using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Document;
using Macro.Extensions;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Macro.Models.ViewModel
{
    public class ConfigEventViewModel
    {
        public string EventType
        {
            get => DocumentHelper.Get(Label.EventType);
        }

        public string EventDataSet
        {
            get => DocumentHelper.Get(Label.EventDataSet);
        }
        public string MouseCoordinates
        {
            get => DocumentHelper.Get(Label.MouseCoordinates);
        }
        public string ConfigList
        {
            get => DocumentHelper.Get(Label.ConfigList);
        }
        public string Config
        {
            get => DocumentHelper.Get(Label.Config);
        }
        public string Save
        {
            get => DocumentHelper.Get(Label.Save);
        }        
    }
}
