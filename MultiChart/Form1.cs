using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MultiChart
{
    public partial class Form1 : Form
    {
        LogManager logManager;
        public Form1()
        {
            InitializeComponent();
            logManager = new LogManager();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> origindata = logManager.ReadLog();
            List<int> origindataTransformed = new List<int>();
            for (int i = 0; i < origindata.Count; i++)
            {
                int data = Convert.ToInt32(origindata[i]);
                origindataTransformed.Add(data);
            }
            List<double> listy = new List<double>();
            List<double> listx = new List<double>();
            //for (int i = 0; i < 270; i++)
            //{
            //    double x = origindataTransformed[i] * Math.Cos((45 + 0.333 * i) * Math.PI / 180);
            //    double y = origindataTransformed[i] * Math.Sin((45 + 0.333 * i) * Math.PI / 180);
            //    listy.Add(y);
            //    listx.Add(x);
            //}
            RandomGenerator randomGenerator = new RandomGenerator();
            List<double> aaa = randomGenerator.Generate();
            for (int i = 1; i <=100; i++)
            {
                listx.Add(i);
                listy.Add(origindataTransformed[i-1]);
            }
            this.chart1.Series[0].Points.Clear();
            InitChart(listx, aaa);
        }

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
            //Series series2 = new Series("S2");
            //series2.ChartArea = "C1";
            //this.chart1.Series.Add(series2);

            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            this.chart1.ChartAreas[0].AxisX.Title = "obstacle data";
            this.chart1.ChartAreas[0].AxisY.Title = "time/ms";
            //设置标题
            //this.chart1.Titles.Clear();
            //this.chart1.Titles.Add("S01");
            //this.chart1.Titles[0].Text = "雷达显示";
            //this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
            //this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            ////设置图表显示样式
            this.chart1.Series[0].Color = Color.Blue;
            this.chart1.Series[0].ChartType = SeriesChartType.Line;
            //this.chart1.Series[1].Color = Color.Blue;
            //this.chart1.Series[1].ChartType = SeriesChartType.Line;
            //List<double> listz = new List<double>();
            //for(int i = 0; i < listx.Count; i++)
            //{
            //    listz.Add(800);
            //}
            for (int i = 0; i < listx.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY(listx[i], listy[i]);
                //this.chart1.Series[1].Points.AddXY(listx[i], listz[i]);
            }
        }
    }
}
