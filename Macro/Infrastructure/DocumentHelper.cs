using Macro.Extensions;
using Macro.Models;
using Utils.Document;
using Utils.Extensions;

namespace Macro.Infrastructure
{
    public class DocumentHelper
    {
        private static Language _language;
        public DocumentHelper()
        {
            _language= ObjectExtensions.GetInstance<IConfig>().Language;
            NotifyHelper.ConfigChanged += (e) =>
            {
                _language = e.Config.Language;
            };
        }
        public static string Get(Label label)
        {
            return DocumentExtensions.Get(label, _language);
        }
        public static string Get(Message mesage)
        {
            return DocumentExtensions.Get(mesage, _language);
        }
    }
}
