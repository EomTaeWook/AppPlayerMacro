using Macro.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Document;

namespace Macro.Infrastructure.Manager
{
    public class ApplicationDataHelper : IDocument
    {
        private Dictionary<string, ApplicationDataModel> _applicationToMap;

        public ApplicationDataHelper()
        {
            _applicationToMap = new Dictionary<string, ApplicationDataModel>();
        }
        public void Init(string filename)
        {
            var list = JsonHelper.Load<IList<ApplicationDataModel>>($"{ConstHelper.DefaultDatasFilePath}{filename}.json");

            foreach(var item in list)
            {
                _applicationToMap.Add(item.Code, item);
            }
        }
        public ApplicationDataModel Find(string name)
        {
            if(_applicationToMap.ContainsKey(name) == true)
            {
                return _applicationToMap[name];
            }
            return null;
        }
    }
}
