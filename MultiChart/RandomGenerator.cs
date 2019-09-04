using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChart
{
    public class RandomGenerator
    {
        public List<double> Generate()
        {
            List<double> list = new List<double>();
            Random random = new Random();
            for(int i = 0; i < 100; i++)
            {
                double k = random.Next(3, 8);
                list.Add(k);
            }
            return list;
        }
    }
}
