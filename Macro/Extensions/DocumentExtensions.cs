using Macro.Models;
using Utils;
using Utils.Document;

namespace Macro.Extensions
{
    public static class DocumentExtensions
    {
        public static string Get(this Label label, Language language)
        {
            return Singleton<DocumentTemplate<Label>>.Instance[label, language];
        }
        public static string Get(this Message mesage, Language language)
        {
            return Singleton<DocumentTemplate<Message>>.Instance[mesage, language];
        }
        public static string Get<Enum>(this IDocument document, Language language, string code) where Enum : struct
        {
            Enum @enum = (Enum)System.Enum.Parse(typeof(Enum), code);
            return Singleton<DocumentTemplate<Enum>>.Instance[@enum, language];
        }
    }
    public class DocumentHelper
    {
        private static Language _language;
        public DocumentHelper(IConfig config) => _language = config.Language;

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
