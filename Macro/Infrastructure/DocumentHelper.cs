using KosherUtils.Framework;
using Macro.Models;
using Utils.Document;
using Utils.Extensions;

namespace Macro.Infrastructure
{
    public class DocumentHelper : Singleton<DocumentHelper>
    {
        private static Language _language;
        public DocumentHelper()
        {
        }
        public void Init(Config config)
        {
            _language = config.Language;
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
        public static string Get(Message mesage, params string[] args)
        {
            var format = DocumentExtensions.Get(mesage, _language);
            return string.Format(format, args);
        }
    }
}
