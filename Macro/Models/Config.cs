using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Document;

namespace Macro.Models
{
    public interface IConfig
    {
        Language Language { get; }
        string SavePath { get; }
        int Period { get; }
    }
    public class Config : IConfig
    {
        public Language Language { get; set; }
        public string SavePath { get; set; }
        public int Period { get; set; } = ConstHelper.MinPeriod;
    }
}
