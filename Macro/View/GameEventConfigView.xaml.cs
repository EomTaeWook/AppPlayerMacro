using Macro.Extensions;
using Macro.Infrastructure;
using Macro.Models.ViewModel;
using Macro.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Macro.View
{
    /// <summary>
    /// GameEventConfigView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameEventConfigView : UserControl
    {
        public TreeGridViewItem CurrentTreeViewItem
        {
            get => this.DataContext<EventConfigViewModel>().CurrentTreeViewItem;
            private set => this.DataContext<EventConfigViewModel>().CurrentTreeViewItem = value;
        }

        private readonly ObservableCollection<KeyValuePair<ConditionType, string>> _conditionItems;
        private readonly TreeGridViewItem _dummyTreeGridViewItem;

        public GameEventConfigView()
        {
            _conditionItems = new ObservableCollection<KeyValuePair<ConditionType, string>>();
            _dummyTreeGridViewItem = new TreeGridViewItem()
            {
                DataContext = new EventConfigViewModel()
            };

            InitializeComponent();

            InitEvent();

            Init();
        }
        private void InitEvent()
        {
            
        }
        private void Init()
        {
            foreach (var type in Enum.GetValues(typeof(ConditionType)))
            {
                if((ConditionType)type == ConditionType.Max)
                {
                    continue;
                }
                if (Enum.TryParse(type.ToString(), out Utils.Document.Label label))
                {
                    _conditionItems.Add(new KeyValuePair<ConditionType, string>((ConditionType)type, DocumentHelper.Get(label)));
                }
            }

            comboHpCondition.ItemsSource = _conditionItems;
            comboHpCondition.DisplayMemberPath = "Value";
            comboHpCondition.SelectedValuePath = "Key";

            comboMpCondition.ItemsSource = _conditionItems;
            comboMpCondition.DisplayMemberPath = "Value";
            comboMpCondition.SelectedValuePath = "Key";
        }
    }
}
