using Macro.Infrastructure.Impl;
using Macro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.View
{
    public partial class GameContentVIew : BaseContentView
    {
        public override void Clear()
        {
            
        }

        public override Task Delete(object state)
        {
            return Task.CompletedTask;
        }

        public override IEnumerable<EventTriggerModel> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override Task Save(object state)
        {
            return Task.CompletedTask;
        }

        public override void SaveDataBind(List<EventTriggerModel> saves)
        {
            
        }
    }
}
