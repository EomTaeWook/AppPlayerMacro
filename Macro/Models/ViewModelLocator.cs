using Macro.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
