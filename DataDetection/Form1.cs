using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DataDetection
{
    public partial class Form1 : Form
    {
        LogManager logManager;
        static List<int> errorIndex;
        static List<double> dis;
        static double bias = 0;
        public Form1()
        {
            InitializeComponent();
            logManager = new LogManager();
            errorIndex = new List<int>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //List<double> time = new List<double>();
            //for (int m = 0; m < 100; m++)
            //{
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bias = Convert.ToDouble(this.textBox3.Text);
            List<string> origindata = logManager.ReadLog(Convert.ToInt32(this.textBox4.Text));
            List<double> origindataTransformed = new List<double>();
            for (int i = 0; i < origindata.Count; i++)
            {
                double data = Convert.ToDouble(origindata[i]);
                origindataTransformed.Add(data);
            }
            
            List<double> listy = new List<double>();
            List<double> listx = new List<double>();
            List<double> listz = new List<double>();
            for (int i = 0; i < 270; i++)
            {
                double x = origindataTransformed[i] * Math.Cos((45 + 0.333 * i) * Math.PI / 180);
                double y = origindataTransformed[i] * Math.Sin((45 + 0.333 * i) * Math.PI / 180);
                listy.Add(y);
                listx.Add(x);
            }
            double d = 0;
            List<double> dis = lineFit3(listy, ref d);
            //double a = 0, b = 0, c = 0;
            //List<double> dis = lineFit(listx, listy, ref a, ref b, ref c);
            //double d = 0;
            //List<double> dis = lineFit2(listy, ref d);
            for (int i = 0; i < listx.Count; i++)
            {
                //double z = (a * listx[i] + c) / (-b);
                double z = d;
                listz.Add(z);
            }
            this.chart1.Series[0].Points.Clear();
            InitChart(listx, listy, listz);
            double difference = Convert.ToDouble(this.textBox3.Text);
            bool isHinder = IsHinder(dis, difference);
            this.label4.Text = this.label4.Text + isHinder.ToString();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            //time.Add(sw.ElapsedMilliseconds);
            //}

        }

        private void InitChart(List<double> listx, List<double> listy, List<double> listz)
        {
            //定义图表区域
            this.chart1.ChartAreas.Clear();
            ChartArea chartArea1 = new ChartArea("C1");
            this.chart1.ChartAreas.Add(chartArea1);
            //定义存储和显示点的容器
            this.chart1.Series.Clear();
            Series series1 = new Series("Laser radar");
            series1.ChartArea = "C1";
            this.chart1.Series.Add(series1);
            Series series2 = new Series("Linear fit");
            series2.ChartArea = "C1";
            this.chart1.Series.Add(series2);
            //设置图表显示样式
            //this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            //this.chart1.ChartAreas[0].AxisY.Maximum = 100;
            //this.chart1.ChartAreas[0].AxisX.Interval = 5;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            //设置标题
            this.chart1.Titles.Clear();
            this.chart1.Titles.Add("S01");
            this.chart1.Titles[0].Text = "Radar Form";
            this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            //设置图表显示样式
            this.chart1.Series[0].Color = Color.Red;
            this.chart1.Series[0].ChartType = SeriesChartType.Line;
            this.chart1.Series[1].Color = Color.Blue;
            this.chart1.Series[1].ChartType = SeriesChartType.Line;
            for (int i = 0; i < listx.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY(listx[i], listy[i]);
                this.chart1.Series[1].Points.AddXY(listx[i], listz[i]);
            }
        }
        /// <summary>
        /// 线性拟合
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static List<double> lineFit(List<double> x, List<double> y, ref double a, ref double b, ref double c)
        {
            //double a, b, c;
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

                    if (d > bias)
                    {
                        dis.Add(d);
                        errorIndex.Add(i);
                    }
                    //Log.WriteLog(LogType.PROCESS, d + "");
                }
            }
            return dis;
        }

        private static List<double> lineFit2(List<double> y, ref double b)
        {
            //double b;
            int size = y.Count;
            if (size < 2)
            {
                b = 0;
                return null;
            }
            double sum = 0;
            for (int i = 0; i < size; i++)
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
                    if (d > bias)
                    {
                        dis.Add(d);
                        errorIndex.Add(i);
                    }
                }
            }
            return dis;
        }

        private static List<double> lineFit3(List<double> y, ref double b)
        {
            double sum = 0;
            double sum_new = 0;
            List<double> y_new = new List<double>();
            for (int i = 0; i < y.Count; i++)
            {
                sum += y[i];
            }
            double a = sum / y.Count;
            for (int i = 0; i < y.Count; i++)
            {
                if (y[i] > a)
                {
                    y_new.Add(y[i]);
                }
            }
            for (int i = 0; i < y_new.Count; i++)
            {
                sum_new += y_new[i];
            }
            b = sum_new / y_new.Count;
            dis = new List<double>();
            errorIndex.Clear();
            for (int i = 0; i < y.Count; i++)
            {
                if (y[i] < b)
                {
                    double d = Math.Abs(y[i] - b);
                    if (d > bias)
                    {
                        dis.Add(d);
                        errorIndex.Add(i);
                    }
                }
            }
            return dis;
        }
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

        private void button1_Click(object sender, EventArgs e)
        {
            Form1_Load(sender, e);
        }
    }
}
