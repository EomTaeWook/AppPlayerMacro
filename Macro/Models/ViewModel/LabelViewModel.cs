using Macro.Extensions;
using Utils.Document;

namespace Macro.Models.ViewModel
{
    public class LabelViewModel
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
        public string TriggerList
        {
            get => DocumentHelper.Get(Label.TriggerList);
        }
        public string Config
        {
            get => DocumentHelper.Get(Label.Config);
        }
        public string Save
        {
            get => DocumentHelper.Get(Label.Save);
        }
        public string SaveConfig
        {
            get => DocumentHelper.Get(Label.TriggerList);
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
        public string Setting
        {
            get => DocumentHelper.Get(Label.Setting);
        }
    }
}
