using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Document;

namespace Macro.Infrastructure.Manager
{
    public class DynamicDPIManager : IDocument
    {
        private IList<DynamicDPI> _applications;
        public DynamicDPIManager()
        {
        }
        public void Init(string filename)
        {
            _applications = JsonHelper.Load<IList<DynamicDPI>>($"{ConstHelper.DefaultDatasFile}{filename}.json");
        }
        public bool Find(string name)
        {
            return _applications.Any(r => r.Code.ToLower().Equals(name.ToLower()));
        }        
    }
}
