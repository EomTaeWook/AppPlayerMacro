using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class SeedRandom
    {
        private Random _random;
        public SeedRandom() : this(DateTime.Now.Ticks)
        {

        }
        public SeedRandom(long seed)
        {
            _random = new Random((int)seed & 0x0000FFFF);
        }
        public double NextDouble()
        {
            return _random.NextDouble();
        }
        public int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }
    }
}
