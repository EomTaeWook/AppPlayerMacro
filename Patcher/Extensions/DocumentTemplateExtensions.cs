using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Document;

namespace Patcher.Extensions
{
    public static class DocumentTemplateExtensions
    {
        public static string Get(this DocumentTemplate<Label> documentTemplate, Label label, Language language)
        {
            return documentTemplate[label, language];
        }

        public static string Get(this DocumentTemplate<Message> documentTemplate, Message message, Language language)
        {
            return documentTemplate[message, language];
        }
    }
}
