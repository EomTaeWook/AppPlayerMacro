using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Document;

namespace Macro.Infrastructure.Manager
{
    public class ApplicationDataManager : IDocument
    {
        private IList<ApplicationData> _applications;
        public ApplicationDataManager()
        {
        }
        public void Init(string filename)
        {
            _applications = JsonHelper.Load<IList<ApplicationData>>($"{ConstHelper.DefaultDatasFile}{filename}.json");
        }
        public ApplicationData Find(string name)
        {
            return _applications.Where(r => r.Code.ToLower().Equals(name.ToLower())).FirstOrDefault();
        }        
    }
}
