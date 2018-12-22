using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public enum Label
    {
        Refresh,
        SelectProcess,
        NotSelectProcessMessage,

        Max
    }
    public class LabelData
    {
        public Label Label { get;set; }
        public string Kor { get; set; }
        public string Eng { get; set; }
    }

    public class LabelDocument : IDocument
    {
        private readonly Dictionary<Label, Dictionary<Language, string>> _labels;
        public LabelDocument()
        {
            _labels = new Dictionary<Label, Dictionary<Language, string>>();
        }
        public void Init(string path)
        {

            var json = File.ReadAllText($@"{path}\{GetType().Name}.json");
            if (!string.IsNullOrEmpty(json))
            {
                var datas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelData>>(json);
                foreach (var data in datas)
                {
                    _labels.Add(data.Label, new Dictionary<Language, string>()
                    {
                        { Language.Kor, data.Kor},
                        { Language.Eng, data.Eng},
                    });
                }
            }
        }
        public string this[Label label, Language language]
        {         
            get
            {
                return _labels[label][language];
            }
        }
    }
}
