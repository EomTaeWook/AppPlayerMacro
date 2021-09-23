using Macro.Infrastructure;
using System;
using System.ComponentModel;

namespace Macro.Models
{
    [Serializable]
    public class ValueConditionModel : INotifyPropertyChanged
    {
        private int _value;
        private ConditionType _conditionType;
        public ValueConditionModel()
        {
            _value = 0;
            _conditionType = ConditionType.Below;
        }
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
        public ConditionType ConditionType
        {
            get => _conditionType;
            set
            {
                _conditionType = value;
                OnPropertyChanged("ConditionType");
            }
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
