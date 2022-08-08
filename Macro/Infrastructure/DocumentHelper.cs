using KosherUtils.Framework;
using Macro.Extensions;
using Macro.Models;
using Utils.Document;
using Utils.Extensions;

namespace Macro.Infrastructure
{
    public class DocumentHelper : Singleton<DocumentHelper>
    {
        private static Language language;
        public DocumentHelper()
        {
        }
        public void Init(Config config)
        {
            language = config.Language;
            NotifyHelper.ConfigChanged += (e) =>
            {
                language = e.Config.Language;
            };
        }
        public static string Get(Label label)
        {
            return DocumentExtensions.Get(label, language);
        }
        public static string Get(Message mesage)
        {
            return DocumentExtensions.Get(mesage, language);
        }
    }
}
