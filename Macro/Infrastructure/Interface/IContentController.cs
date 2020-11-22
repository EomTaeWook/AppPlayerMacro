using Macro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Infrastructure.Interface
{
    interface IContentController
    {
        Task<Tuple<bool, IBaseEventTriggerModel>> TriggerProcess<T>(T model, ProcessConfigModel processConfigModel) where T : BaseEventTriggerModel<T>;
    }
}
