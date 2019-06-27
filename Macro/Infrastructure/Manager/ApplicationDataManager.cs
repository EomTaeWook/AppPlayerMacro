using Macro.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Document;

namespace Macro.Infrastructure.Manager
{
    public class ApplicationDataManager : IDocument
    {
        private IList<ApplicationDataModel> _applications;
        public ApplicationDataManager()
        {
        }
        public void Init(string filename)
        {
            _applications = JsonHelper.Load<IList<ApplicationDataModel>>($"{ConstHelper.DefaultDatasFile}{filename}.json");
        }
        public ApplicationDataModel Find(string name)
        {
            return _applications.Where(r => r.Code.ToLower().Equals(name.ToLower())).FirstOrDefault();
        }        
    }
}
