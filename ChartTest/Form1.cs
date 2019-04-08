using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartTest
{
    public partial class Form1 : Form
    {
        private static bool isover = false;
        public Form1()
        {
            InitializeComponent();                       
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isover = false;
            Task.Factory.StartNew(() =>
            {
                while (!isover)
                {
                    this.Invoke(new Action(() =>
                    {
                        List<string> origindata = ReadLog();
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        List<double> listx = new List<double>();
                        List<double> listy = new List<double>();
                        int startDegree = (Convert.ToInt32(this.textBox1.Text) + 45) * 3;
                        int endDegree = (Convert.ToInt32(this.textBox2.Text) + 45) * 3 + 1;                        
                        List<int> origindataTransformed = new List<int>();
                        origindataTransformed = origindata.ConvertAll<int>(x => Convert.ToInt32(x));
                        for (int i = startDegree; i < endDegree; i++)
                        {
                            double x = origindataTransformed[i] * Math.Cos((-45 + 0.333 * i) * Math.PI / 180);
                            double y = origindataTransformed[i] * Math.Sin((-45 + 0.333 * i) * Math.PI / 180);
                            listx.Add(x);
                            listy.Add(y);
                        }
                        this.chart1.Series[0].Points.Clear();
                        InitChart(listx,listy);
                        sw.Stop();
                        Console.WriteLine(sw.ElapsedMilliseconds + "ms");
                    }));
                    Thread.Sleep(5000);
                }
            });
            //List<string> origindata = ReadLog();
            //List<int> origindataTransformed = new List<int>();
            //origindataTransformed = origindata.ConvertAll<int>(x => Convert.ToInt32(x));
            //for(int i = 0; i < origindataTransformed.Count; i++)
            //{
            //    double x = origindataTransformed[i] * Math.Cos((-45 + 0.333 * i) * Math.PI / 180);
            //    double y = origindataTransformed[i] * Math.Sin((-45 + 0.333 * i) * Math.PI / 180);
            //    listx.Add(x);
            //    listy.Add(y);
            //}
            //InitChart();

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

        private void button2_Click(object sender, EventArgs e)
        {
            isover = true;
        }
    }
}
