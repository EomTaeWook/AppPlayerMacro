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
        private bool _isImageSearchRequired;
        public GameEventTriggerModel()
        {
            _isImageSearchRequired = true;
            _hpCondition = new ValueConditionModel();
            _mpCondition = new ValueConditionModel();
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
            _isImageSearchRequired = model.IsImageSearchRequired;
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
        [Order(14)]
        public override bool IsImageSearchRequired
        {
            set
            {
                _isImageSearchRequired = value;
                OnPropertyChanged("IsImageSearchRequired");
            }
            get => _isImageSearchRequired;
        }
    }
}
