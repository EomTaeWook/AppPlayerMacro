using KosherUtils.Framework;
using Macro.Models.ViewModel;
using Unity;
using Utils;

namespace Macro.Models
{
    public class ViewModelLocator
    {
        public LabelViewModel LabelViewModel
        {
            get => Singleton<UnityContainer>.Instance.Resolve<LabelViewModel>();
        }
        public CommonEventConfigViewModel CommonEventConfigViewModel
        {
            get => Singleton<UnityContainer>.Instance.Resolve<CommonEventConfigViewModel>();
        }
        public GameEventConfigViewModel GameEventConfigViewModel
        {
            get => Singleton<UnityContainer>.Instance.Resolve<GameEventConfigViewModel>();
        }
        public SettingViewModel SettingViewModel
        {
            get => Singleton<UnityContainer>.Instance.Resolve<SettingViewModel>(); 
        }
    }
}
