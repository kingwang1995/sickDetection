using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CheckTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> dd = ReadLog();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            List<double> listy = new List<double>();
            List<double> listx = new List<double>();
            List<int> vadd = new List<int>();
            vadd = dd.ConvertAll<int>(x => Convert.ToInt32(x));
            for (int k = 166; k < 646; k++)
            {
                double x = vadd[k] * Math.Sin((-135 + 0.33 * k) * Math.PI / 180);
                double y = vadd[k] * Math.Cos((-135 + 0.33 * k) * Math.PI / 180);
                listy.Add(y);
                listx.Add(x);
            }
            List<double> dis = lineFit(listx, listy);
            bool isHinder = IsHinder(dis, 20);
            sw.Stop();
            Console.WriteLine("结果："+isHinder);
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            Console.Read();
        }

        private static List<string> ReadLog()
        {
            List<string> digitList = new List<string>();
            string filePath = "log//data0.txt";
            try
            {
                List<string> logLines = null;
                if (File.Exists(filePath))
                {
                    logLines = new List<string>(File.ReadAllLines(filePath));
                }
                if (logLines != null)
                {
                    for (int i = 0; i < logLines.Count; i++)
                    {
                        string logLine = logLines[i];
                        digitList.Add(logLine);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return digitList;
        }

        private static List<double> lineFit(List<double> x, List<double> y)
        {
            double a, b, c;
            int size = x.Count;
            if (size < 2)
            {
                a = 0;
                b = 0;
                c = 0;
                return null;
            }
            double x_mean = 0;
            double y_mean = 0;
            for (int i = 0; i < size; i++)
            {
                x_mean += x[i];
                y_mean += y[i];
            }
            x_mean /= size;
            y_mean /= size; //至此，计算出了 x y 的均值

            double Dxx = 0, Dxy = 0, Dyy = 0;
            for (int i = 0; i < size; i++)
            {
                Dxx += (x[i] - x_mean) * (x[i] - x_mean);
                Dxy += (x[i] - x_mean) * (y[i] - y_mean);
                Dyy += (y[i] - y_mean) * (y[i] - y_mean);
            }

            double lambda = ((Dxx + Dyy) - Math.Sqrt((Dxx - Dyy) * (Dxx - Dyy) + 4 * Dxy * Dxy)) / 2.0;
            double den = Math.Sqrt(Dxy * Dxy + (lambda - Dxx) * (lambda - Dxx));
            a = Dxy / den;
            b = (lambda - Dxx) / den;
            c = -a * x_mean - b * y_mean;
            Console.WriteLine(a+"  "+b+"  "+c);
            List<double> dis = new List<double>();
            for (int i = 0; i < size; i++)
            {
                double d = Math.Abs(a * x[i] + b * y[i] + c) / Math.Sqrt(a * a + b * b);
                dis.Add(d);
            }
            return dis;
        }

        private static bool IsHinder(List<double> testdata, double deviation)
        {
            bool isHinder = true;
            //Data errordata = new Data();
            List<int> recorddata = new List<int>();
            for(int i = 0; i < testdata.Count; i++)
            {
                if (testdata[i] > deviation)
                {
                    //errordata.key = i;
                    //errordata.value = testdata[i];
                    Console.WriteLine(i+" "+testdata[i]);
                    recorddata.Add(i);
                }
            }
            if (recorddata.Count <= 5||IsBorderUpon(recorddata,5)<1)
            {
                isHinder = false;
            }
            return isHinder;
        }

        private static int IsBorderUpon(List<int> arr ,int num)
        {
            var query = arr.OrderBy(p => p).Aggregate<int, List<List<int>>>(null, (m, n) =>
            {
                if (m == null) return new List<List<int>>() { new List<int>() { n } };
                if (m.Last().Last() != n - 1)
                {
                    m.Add(new List<int>() { n });
                }
                else
                {
                    m.Last().Add(n);
                }
                return m;
            });
            int flag = 0;
            for (int i = 0; i < query.Count; i++)
            {
                if (query[i].Count > 5)
                {
                    flag++;
                }
            }
            return flag;
        }

    }
}
