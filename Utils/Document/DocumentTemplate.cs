using System;
using System.Collections.Generic;
using System.IO;

namespace Utils.Document
{
    public class DocumentTemplate<T> : IDocument where T : struct //7.3 Enum
    {
        private readonly Dictionary<T, Dictionary<Language, string>> datas;
        public DocumentTemplate()
        {
            datas = new Dictionary<T, Dictionary<Language, string>>();
        }
        public void Init(string path)
        {
            var json = File.ReadAllText($@"{path}{typeof(T).Name}Document.json");
            if (string.IsNullOrEmpty(json) == false)
            {
                var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DocumentData>>(json);
                foreach (var data in jsonData)
                {
                    if(Enum.TryParse(data.Code, true, out T code))
                    {
                        datas.Add(code, new Dictionary<Language, string>()
                        {
                            { Language.Kor, data.Kor},
                            { Language.Eng, data.Eng},
                        });
                    }
                }
            }
        }
        public string this[T code, Language language] => datas[code][language];
    }
}
