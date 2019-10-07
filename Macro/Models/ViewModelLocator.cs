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
        public EventConfigViewModel ConfigEventViewModel
        {
            get => Singleton<UnityContainer>.Instance.Resolve<EventConfigViewModel>();
        }
        public SettingViewModel SettingViewModel
        {
            get => Singleton<UnityContainer>.Instance.Resolve<SettingViewModel>(); 
        }
    }
}
