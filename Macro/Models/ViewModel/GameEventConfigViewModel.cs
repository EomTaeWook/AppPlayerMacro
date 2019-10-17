namespace Macro.Models.ViewModel
{
    public class GameEventConfigViewModel : BaseEventConfigViewModel<GameEventTriggerModel>
    {
        private ValueConditionModel _hpCondition;
        private ValueConditionModel _mpCondition;
        
        public ValueConditionModel HpCondition
        {
            set
            {
                _hpCondition = value;
                OnPropertyChanged("HpConditionModel");
            }
            get => _hpCondition;
        }
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
