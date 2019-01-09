using Utils;
using Utils.Document;

namespace Macro.Models
{
    public interface IConfig
    {
        Language Language { get; }
        string SavePath { get; }
        int Period { get; }
        int ProcessDelay { get; }
        int Similarity { get; }
    }

    public class Config : IConfig
    {
        public Language Language { get; set; }
        public string SavePath { get; set; }
        public int Period { get; set; } = ConstHelper.MinPeriod;
        public int ProcessDelay { get; set; } = ConstHelper.ProcessDelay;
        public int Similarity { get; set; } = ConstHelper.DefaultSimilarity;
    }
}
