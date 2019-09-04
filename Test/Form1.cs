using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Test
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        NetworkStream stream;
        private int bufferSize = 4096;
        private string startCmd = "02 73 52 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 03";
        private string endCmd = "02 73 45 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 30 03";
        int flag = 0;
        private bool isok = false;
        private static readonly object Lock = new object();
        List<string> validdatalist;
        static List<int> errorIndex;
        static List<double> dis;
        int indexN = 0;
        public Form1()
        {
            InitializeComponent();
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("192.168.1.31"), 2111);
            Console.WriteLine("连接成功");
            stream = tcpClient.GetStream();
            validdatalist = new List<string>();
            errorIndex = new List<int>();
        }
        //开始
        private void button1_Click(object sender, EventArgs e)
        {
            isok = false;
            List<string> datalist = new List<string>();
            Task.Factory.StartNew(() =>
            {
                while (!isok)
                {
                    lock (Lock)
                    {
                        datalist.Clear();
                        flag = sendCmd(startCmd);
                        byte[] buffer = new byte[bufferSize];
                        stream.Read(buffer, 0, buffer.Length);
                        string str = HexStringToASCII(buffer);
                        datalist.Add(str);
                    }
                    Thread.Sleep(200);
                }
            });
            //string testimg = "";
            Task.Factory.StartNew(() =>
            {
                while (!isok)
                {
                    lock (Lock)
                    {
                        for (int i = 0; i < datalist.Count; i++)
                        {
                            string ss = validStr(datalist[i], "sRA LMDscandata 1 ", " not defined 0 0 0");
                            string[] data = ss.Split(' ');
                            string number = data[22];
                            int a = Convert.ToInt32(number, 16);
                            int[] validNum = new int[a];
                            string aws = "";
                            for (int j = 0; j < a; j++)
                            {
                                validNum[j] = Convert.ToInt32(data[23 + j], 16);
                                aws = aws + validNum[j] + " ";
                            }
                            validdatalist.Add(aws);

                            this.Invoke(new Action(() =>
                            {
                                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                                sw.Start();
                                aws.TrimEnd(' ');
                                List<double> listy = new List<double>();
                                List<double> listx = new List<double>();
                                string[] origindata = aws.Split(' ');
                                origindata = origindata.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                                int[] origindataTransformed = Array.ConvertAll<string, int>(origindata, int.Parse);
                                int startDegree = (Convert.ToInt32(this.textBox1.Text) + 45) * 3;
                                int endDegree = (Convert.ToInt32(this.textBox2.Text) + 45) * 3 + 1;
                                for (int k = startDegree; k < endDegree; k++)
                                {
                                    double x = origindataTransformed[k] * Math.Cos((-45 + 0.333 * k) * Math.PI / 180);
                                    double y = origindataTransformed[k] * Math.Sin((-45 + 0.333 * k) * Math.PI / 180);
                                    listy.Add(y);
                                    listx.Add(x);
                                }
                                this.chart1.Series[0].Points.Clear();
                                InitChart(listx, listy);
                                //List<double> dis = lineFit(listx, listy);
                                List<double> dis = lineFit2(listy);
                                double difference = Convert.ToDouble(this.textBox3.Text);
                                bool isHinder = IsHinder(dis, difference);
                                this.label4.Text = isHinder.ToString();
                                sw.Stop();
                                Console.WriteLine(sw.ElapsedMilliseconds + "ms");
                                if (isHinder)
                                {
                                    Console.WriteLine("结果：" + isHinder);
                                    Console.WriteLine(sw.ElapsedMilliseconds + "ms");

                                    for (int j = 0; j < listx.Count; j++)
                                    {
                                        Log.WriteData("ordata" + indexN, origindataTransformed[startDegree + j].ToString());
                                        if (errorIndex.Exists(o => o == j + 1))
                                        {
                                            Log.WriteData("data" + indexN, origindataTransformed[startDegree + j] + " " + 1);
                                        }
                                        else
                                        {
                                            Log.WriteData("data" + indexN, origindataTransformed[startDegree + j] + " " + 0);
                                        }
                                    }
                                    indexN++;
                                }                                
                            }));

                        }
                    }
                    Thread.Sleep(10);
                }
            });

        }

        private int sendCmd(string cmd)
        {
            if (cmd.Equals(startCmd))
            {
                byte[] byteStartCmd = HexStringToBinary(startCmd);
                string result = HexStringToASCII(byteStartCmd);
                byte[] b = Encoding.UTF8.GetBytes(result);
                stream.Write(b, 0, b.Length);
                return 1;
            }
            if (cmd.Equals(endCmd))
            {
                byte[] byteStartCmd = HexStringToBinary(endCmd);
                string result = HexStringToASCII(byteStartCmd);
                byte[] b = Encoding.UTF8.GetBytes(result);
                stream.Write(b, 0, b.Length);
                return 2;
            }
            return 0;
        }
        /// <summary>
        /// 将一条十六进制字符串转换为ASCII
        /// </summary>
        /// <param name="hexstring">一条十六进制字符串</param>
        /// <returns>返回一条ASCII码</returns>
        private static string HexStringToASCII(byte[] bt)
        {
            //byte[] bt = HexStringToBinary(hexstring);
            string lin = "";
            for (int i = 0; i < bt.Length; i++)
            {
                lin = lin + bt[i] + " ";
            }


            string[] ss = lin.Trim().Split(new char[] { ' ' });
            char[] c = new char[ss.Length];
            int a;
            for (int i = 0; i < c.Length; i++)
            {
                a = Convert.ToInt32(ss[i]);
                c[i] = Convert.ToChar(a);
            }

            string b = new string(c);
            return b;
        }

        /// <summary>
        /// 16进制字符串转换为二进制数组
        /// </summary>
        /// <param name="hexstring">用空格切割字符串</param>
        /// <returns>返回一个二进制字符串</returns>
        private static byte[] HexStringToBinary(string hexstring)
        {

            string[] tmpary = hexstring.Trim().Split(' ');
            byte[] buff = new byte[tmpary.Length];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = Convert.ToByte(tmpary[i], 16);
            }
            return buff;
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="sourse"></param>
        /// <param name="startstr"></param>
        /// <param name="endstr"></param>
        /// <returns></returns>
        private static string validStr(string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                {
                    return result;
                }
                string tmpstr = sourse.Substring(startindex + startstr.Length);
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                {
                    return result;
                }
                result = tmpstr.Remove(endindex);
            }
            catch (Exception) { }
            return result;
        }


        /// <summary>
        /// 初始化图表
        /// </summary>
        private void InitChart(List<double> listx, List<double> listy)
        {
            //定义图表区域
            this.chart1.ChartAreas.Clear();
            ChartArea chartArea1 = new ChartArea("C1");
            this.chart1.ChartAreas.Add(chartArea1);
            //定义存储和显示点的容器
            this.chart1.Series.Clear();
            Series series1 = new Series("S1");
            series1.ChartArea = "C1";
            this.chart1.Series.Add(series1);
            //设置图表显示样式
            //this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            //this.chart1.ChartAreas[0].AxisY.Maximum = 100;
            //this.chart1.ChartAreas[0].AxisX.Interval = 5;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            this.chart1.Titles.Clear();
            this.chart1.Titles.Add("S01");
            this.chart1.Titles[0].Text = "雷达显示";
            this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.chart1.Series[0].Color = Color.Red;
            this.chart1.Series[0].ChartType = SeriesChartType.Line;
            for (int i = 0; i < listx.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY(listx[i], listy[i]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isok = true;
            for (int i = 0; i < validdatalist.Count; i++)
            {
                string temp = validdatalist[i];
                string[] tempdata = temp.Split(' ');
                for (int j = 0; j < tempdata.Length; j++)
                {
                    Log.WriteData("data" + i, tempdata[j]);
                }
            }
            Environment.Exit(0);
            this.Close();
        }

        /// <summary>
        /// 线性拟合
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
            Console.WriteLine(a + "  " + b + "  " + c);
            dis = new List<double>();
            errorIndex.Clear();
            for (int i = 0; i < size; i++)
            {
                if (y[i] < (-a * x[i] - c) / b)
                {
                    double d = Math.Abs(a * x[i] + b * y[i] + c) / Math.Sqrt(a * a + b * b);
                    
                    if (d > 40)
                    {
                        dis.Add(d);
                        errorIndex.Add(i);
                    }
                    //Log.WriteLog(LogType.PROCESS, d + "");
                }
            }
            return dis;
        }
        /// <summary>
        /// 均值法
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private static List<double> lineFit2(List<double> y)
        {
            double b;
            int size = y.Count;
            if (size < 2)
            {
                b = 0;
                return null;
            }
            double sum = 0;
            for(int i = 0; i < size; i++)
            {
                sum += y[i];
            }
            b = sum / size;
            dis = new List<double>();
            errorIndex.Clear();
            for (int i = 0; i < size; i++)
            {
                if (y[i] < b)
                {
                    double d = Math.Abs(y[i] - b);
                    if (d > 40)
                    {
                        dis.Add(d);
                        errorIndex.Add(i);
                    }
                }
            }
            return dis;
        }
        /// <summary>
        /// 是否有障碍物
        /// </summary>
        /// <param name="testdata"></param>
        /// <param name="deviation"></param>
        /// <returns></returns>

        private static bool IsHinder(List<double> testdata, double deviation)
        {
            bool isHinder = true;
            //Data errordata = new Data();
            List<int> recorddata = new List<int>();
            for (int i = 0; i < testdata.Count; i++)
            {
                if (testdata[i] > deviation)
                {
                    //errordata.key = i;
                    //errordata.value = testdata[i];
                    //Console.WriteLine(i + " " + testdata[i]);
                    recorddata.Add(i);
                }
            }
            if (recorddata.Count <= 5 || IsBorderUpon(recorddata, 10) < 1)
            {
                isHinder = false;
            }
            return isHinder;
        }
        /// <summary>
        /// 连续判断
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="num"></param>
        /// <returns></returns>

        private static int IsBorderUpon(List<int> arr, int num)
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

        private void button3_Click(object sender, EventArgs e)
        {
            isok = true;
        }
    }
}