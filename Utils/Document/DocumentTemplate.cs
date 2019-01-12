using System;
using System.Collections.Generic;
using System.IO;

namespace Utils.Document
{
    public class DocumentTemplate<T> : IDocument where T : struct //7.3 Enum
    {
        private readonly Dictionary<T, Dictionary<Language, string>> _datas;
        public DocumentTemplate()
        {
            _datas = new Dictionary<T, Dictionary<Language, string>>();
        }
        public void Init(string path)
        {
            var json = File.ReadAllText($@"{path}\{typeof(T).Name}Document.json");
            if (!string.IsNullOrEmpty(json))
            {
                var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DocumentData>>(json);
                foreach (var data in jsonData)
                {
                    T code = (T)Enum.Parse(typeof(T), data.Code, true);
                    _datas.Add(code, new Dictionary<Language, string>()
                    {
                        { Language.Kor, data.Kor},
                        { Language.Eng, data.Eng},
                    });
                }
            }
        }
        public string this[T code, Language language]
        {
            get
            {
                return _datas[code][language];
            }
        }
    }
}
