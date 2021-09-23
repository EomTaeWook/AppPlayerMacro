using Macro.Models;
using System;
using System.Threading.Tasks;

namespace Macro.Infrastructure.Interface
{
    interface IContentController
    {
        Task<Tuple<bool, IBaseEventTriggerModel>> TriggerProcess<T>(T model, ProcessConfigModel processConfigModel) where T : BaseEventTriggerModel<T>;
    }
}
