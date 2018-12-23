using Macro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Document;

namespace Macro.Extensions
{
    public static class DocumentExtensions
    {
        public static string Get(this IDocument document, Language language, string key)
        {
            if (Enum.TryParse(key, out Label label))
            {
                return Singleton<LabelDocument>.Instance[label, language];
            }
            else
                return null;
        }
    }
}
