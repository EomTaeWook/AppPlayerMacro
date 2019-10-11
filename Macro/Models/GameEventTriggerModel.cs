using Macro.Extensions;
using Macro.Infrastructure.Serialize;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Macro.Models
{
    [Serializable]
    public class GameEventTriggerModel : BaseEventTriggerModel<GameEventTriggerModel>
    {
        private ValueConditionModel _hpCondition;
        private ValueConditionModel _mpCondition;
        public GameEventTriggerModel()
        {

        }
        public GameEventTriggerModel(GameEventTriggerModel model)
        {
            Image = model.Image.Clone() as Bitmap;
            EventType = model.EventType;
            MouseTriggerInfo = model.MouseTriggerInfo.Clone();
            MonitorInfo = model.MonitorInfo.Clone();
            KeyboardCmd = model.KeyboardCmd.Clone() as string;
            ProcessInfo = model.ProcessInfo.Clone();
            _subEventTriggers = new ObservableCollection<GameEventTriggerModel>();
            foreach (var item in model.SubEventTriggers)
            {
                _subEventTriggers.Add(item);
            }
            AfterDelay = model.AfterDelay;
            RepeatInfo = model.RepeatInfo.Clone();
            EventToNext = model.EventToNext;
            HpCondition = model.HpCondition.Clone();
            MpCondition = model.MpCondition.Clone();
            _triggerIndex = 0;
        }
        [Order(12)]
        public ValueConditionModel HpCondition
        {
            set
            {
                _hpCondition = value;
                OnPropertyChanged("HpConditionModel");
            }
            get => _hpCondition;
        }
        [Order(13)]
        public ValueConditionModel MpCondition
        {
            set
            {
                _mpCondition = value;
                OnPropertyChanged("MpCondition");
            }
            get => _mpCondition;
        }
    }
}
