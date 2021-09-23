using Macro.Infrastructure.Interface;
using Macro.Models;
using System;
using System.Threading.Tasks;
using Utils.Infrastructure;

namespace Macro.Infrastructure.Controller
{
    public class GameContentController : IContentController
    {
        public async Task<Tuple<bool, IBaseEventTriggerModel>> TriggerProcess<T>(T model, ProcessConfigModel processConfigModel) where T : BaseEventTriggerModel<T>
        {
            var isExcute = true;
            await TaskHelper.TokenCheckDelayAsync(processConfigModel.ItemDelay, processConfigModel.Token);
            return Tuple.Create<bool, IBaseEventTriggerModel>(isExcute, null);
        }
    }
}
