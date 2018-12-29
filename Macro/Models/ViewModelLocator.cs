using Macro.Models.ViewModel;
using Unity;
using Utils;

namespace Macro.Models
{
    public class ViewModelLocator
    {
        public MainWindowViewModel MainWindowViewModel
        {
            get { return Singleton<UnityContainer>.Instance.Resolve<MainWindowViewModel>(); }
        }
        public ConfigEventViewModel ConfigEventViewModel
        {
            get { return Singleton<UnityContainer>.Instance.Resolve<ConfigEventViewModel>(); }
        }
    }
}
