using Macro.Models.ViewModel;
using Unity;
using Utils;

namespace Macro.Models
{
    public class ViewModelLocator
    {
        public LabelViewModel LabelViewModel
        {
            get { return Singleton<UnityContainer>.Instance.Resolve<LabelViewModel>(); }
        }
        public ConfigEventViewModel ConfigEventViewModel
        {
            get { return Singleton<UnityContainer>.Instance.Resolve<ConfigEventViewModel>(); }
        }
    }
}
