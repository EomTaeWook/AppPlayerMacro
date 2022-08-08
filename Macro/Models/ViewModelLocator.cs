using KosherUtils.Framework;
using Macro.Infrastructure.Manager;
using Macro.Models.ViewModel;

namespace Macro.Models
{
    public class ViewModelLocator
    {
        public LabelViewModel LabelViewModel
        {
            get => ServiceProviderManager.Instance.GetService<LabelViewModel>();
        }
        public SettingViewModel SettingViewModel
        {
            get=> ServiceProviderManager.Instance.GetService<SettingViewModel>();
        }
    }
}
