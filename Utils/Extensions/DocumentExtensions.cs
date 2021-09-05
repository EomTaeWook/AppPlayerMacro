using KosherUtils.Framework;
using Utils.Document;

namespace Utils.Extensions
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
            if (document == null)
            {
                throw new System.ArgumentNullException(nameof(document));
            }

            Enum @enum = (Enum)System.Enum.Parse(typeof(Enum), code);
            return Singleton<DocumentTemplate<Enum>>.Instance[@enum, language];
        }
    }
}
