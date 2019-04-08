using System;
using System.Collections.Generic;
using System.Linq;

namespace BorderUpon_
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] arr = { 1, 2, 3, 5, 7, 8, 9, 15, 18, 19, 24 };
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
            //List<int> aa = new List<int>();
            int flag = 0;
            for (int i = 0; i < query.Count; i++)
            {
                if (query[i].Count > 5)
                {
                    flag++;
                }
            }
            //打印结果
            //query.ForEach(p =>
            //{
            //    if (p.Count > 5)
            //    {
            //        Console.WriteLine("true");
            //    }
            //    else
            //    {
            //        Console.WriteLine("false");
            //    }
            //});
            //Console.Read();
        }
        private static bool IsBorderUpon(int[] arr)
        {
            int max_num = 0;
            int min_num = 65535;
            int zero_count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (arr[i] == 0)
                {
                    zero_count++;
                }
                else
                {
                    if (arr[i] > max_num)
                    {
                        max_num = arr[i];
                    }
                    if (arr[i] < min_num)
                    {
                        min_num = arr[i];
                    }
                }
            }

            if (zero_count == 4 || zero_count == 5)
            {
                return true;
            }
            else if (zero_count == 0)
            {
                if ((max_num - min_num) == 4)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if ((max_num - min_num) <= 4)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
