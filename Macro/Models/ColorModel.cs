using System.Collections.Generic;

namespace Macro.Models
{
    public class ColorModel
    {
        public string Code { get; set; }

        public RGBData Lower { get; set; }

        public RGBData Upper { get; set; }
    }
    public class RGBData
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }
    }
}