using Macro.Extensions;
using Macro.Infrastructure.Manager;
using Macro.Infrastructure.Serialize;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utils;
using Utils.Document;
using Utils.Extensions;
using Utils.Infrastructure;
using Point = System.Windows.Point;

namespace Macro.Infrastructure.Impl
{
    public abstract class BaseContentView : UserControl
    {
        public abstract Task Save(object state);

        public abstract Task Delete(object state);

        public abstract bool Validate(IBaseEventTriggerModel model, out Message error);

        public abstract void Clear();
        public abstract Task Load(object state);
 
        public abstract IEnumerable<IBaseEventTriggerModel> GetEnumerator();

        public abstract Task<IBaseEventTriggerModel> InvokeNextEventTriggerAsync(IBaseEventTriggerModel saveModel, ProcessConfigModel processEventTriggerModel);

        //public async Task<IBaseEventTriggerModel> InvokeNextEventTriggerAsync(IBaseEventTriggerModel saveModel, ProcessConfigModel processEventTriggerModel)
        //{
        //    if (processEventTriggerModel.Token.IsCancellationRequested)
        //        return null;
        //    var nextModel = await TriggerProcess(saveModel, processEventTriggerModel);
        //    return nextModel.Item2;
        //}
        public abstract void CaptureImage(Bitmap bmp);
     }
}
