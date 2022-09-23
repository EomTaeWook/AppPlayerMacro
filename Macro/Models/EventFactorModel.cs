using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro.Models
{
    public struct FactorModel
    {
        public float FactorX { get; set; }

        public float FactorY { get; set; }
    }

    public struct EventFactorModel
    {
        public FactorModel Factor { get; set; }

        public FactorModel PositionFactor { get; set; }
    }
}
