using Macro.Infrastructure.Manager;
using Macro.Models.ViewModel;

namespace Macro.Models
{
    public class ViewModelLocator
    {
        public LabelViewModel LabelViewModel
        {
            get => ServiceDispatcher.Resolve<LabelViewModel>();
        }
        public SettingViewModel SettingViewModel
        {
            get=> ServiceDispatcher.Resolve<SettingViewModel>();
        }
    }
}
