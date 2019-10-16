namespace Macro.Models.ViewModel
{
    public class GameEventConfigViewModel : BaseEventConfigViewModel<GameEventTriggerModel>
    {
        private ValueConditionModel _hpCondition;
        private ValueConditionModel _mpCondition;
        private double _currentWindowWidth = 0;

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
        public double CurrentWindowWidth {
            set
            {
                _currentWindowWidth = value;
                OnPropertyChanged("CurrentWindowWidth");
            }
            get => _currentWindowWidth;
        }
    }
}
