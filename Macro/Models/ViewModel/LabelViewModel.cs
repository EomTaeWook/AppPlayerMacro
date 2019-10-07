using Macro.Infrastructure;
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
        public string MouseWheel
        {
            get => DocumentHelper.Get(Label.MouseWheel);
        }
        public string WheelData
        {
            get => DocumentHelper.Get(Label.WheelData);
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
        public string Title
        {
            get => $"{DocumentHelper.Get(Label.Title)} ver {Version.CurrentVersion.Major}.{Version.CurrentVersion.Minor}.{Version.CurrentVersion.Build}";
        }
        public string Language
        {
            get => DocumentHelper.Get(Label.Language);
        }
        public string SavePath
        {
            get => DocumentHelper.Get(Label.SavePath);
        }
        public string Period
        {
            get => DocumentHelper.Get(Label.Period);
        }
        public string ItemDelay
        {
            get => DocumentHelper.Get(Label.ItemDelay);
        }
        public string Similarity
        {
            get => DocumentHelper.Get(Label.Similarity);
        }
        public string AfterDelay
        {
            get => DocumentHelper.Get(Label.AfterDelay);
        }
        public string SearchResultDisplay
        {
            get => DocumentHelper.Get(Label.SearchResultDisplay);
        }
        public string VersionCheck
        {
            get => DocumentHelper.Get(Label.VersionCheck);
        }
        public string Fix
        {
            get => DocumentHelper.Get(Label.Fix);
        }
        public string RepeatSubItems
        {
            get => DocumentHelper.Get(Label.RepeatSubItems);
        }
        public string Cancel
        {
            get => DocumentHelper.Get(Label.Cancel);
        }
        public string AddSameContent
        {
            get => DocumentHelper.Get(Label.AddSameContent);
        }
        public string TriggerToNext
        {
            get => DocumentHelper.Get(Label.TriggerToNext);
        }
        public string X
        {
            get => "X";
        }
        public string Y
        {
            get => "Y";
        }
        public string Common
        {
            get => DocumentHelper.Get(Label.Common);
        }
        public string Game
        {
            get => DocumentHelper.Get(Label.Game);
        }
        public string HP
        {
            get => "Hp";
        }

        public string MP
        {
            get => "Mp";
        }

        public string HpCondition
        {
            get => DocumentHelper.Get(Label.HpCondition);
        }

        public string MpCondition
        {
            get => DocumentHelper.Get(Label.MpCondition);
        }

    }
}
