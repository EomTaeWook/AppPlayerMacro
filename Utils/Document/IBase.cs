using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Document
{
    public enum Language
    {
        Kor,
        Eng,

        Max
    }
    public class DocumentData
    {
        public string Code { get; set; }
        public string Kor { get; set; }
        public string Eng { get; set; }
    }

    public interface IDocument
    {
        void Init(string path);
    }
}
