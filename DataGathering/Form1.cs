﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace DataGathering
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        NetworkStream stream;
        private int bufferSize = 4096;
        private string startCmd = "02 73 52 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 03";
        int flag = 0;
        private bool isok = false;
        private static readonly object Lock = new object();
        List<string> validdatalist;
        public Form1()
        {
            InitializeComponent();
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("192.168.1.31"), 2111);
            Console.WriteLine("连接成功");
            stream = tcpClient.GetStream();
            validdatalist = new List<string>();
        }

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
                                //int startDegree = (Convert.ToInt32(this.textBox1.Text) + 45) * 3;
                                //int endDegree = (Convert.ToInt32(this.textBox2.Text) + 45) * 3 + 1;
                                for (int k = 0; k < origindataTransformed.Length; k++)
                                {
                                    double x = origindataTransformed[k] * Math.Cos((45 + 0.333 * k) * Math.PI / 180);
                                    double y = origindataTransformed[k] * Math.Sin((45 + 0.333 * k) * Math.PI / 180);
                                    Log.WriteCredence(x + " " + y);
                                    listy.Add(y);
                                    listx.Add(x);
                                }
                                this.chart1.Series[0].Points.Clear();

                                sw.Stop();
                                Console.WriteLine(sw.ElapsedMilliseconds + "ms");
                            }));
                        }
                    }
                    Thread.Sleep(200);
                }
            });
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
            this.chart1.Series[0].ChartType = SeriesChartType.Point;
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
            return 0;
        }

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

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> datalist = new List<string>();
            datalist.Clear();
            flag = sendCmd(startCmd);
            byte[] buffer = new byte[bufferSize];
            stream.Read(buffer, 0, buffer.Length);
            string str = HexStringToASCII(buffer);
            datalist.Add(str);
            string ss = validStr(datalist[0], "sRA LMDscandata 1 ", " not defined 0 0 0");
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
            aws.TrimEnd(' ');
            List<double> listy = new List<double>();
            List<double> listx = new List<double>();
            string[] origindata = aws.Split(' ');
            origindata = origindata.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            int[] origindataTransformed = Array.ConvertAll<string, int>(origindata, int.Parse);
            //int startDegree = (Convert.ToInt32(this.textBox1.Text) + 45) * 3;
            //int endDegree = (Convert.ToInt32(this.textBox2.Text) + 45) * 3 + 1;
            for (int k = 0; k < origindataTransformed.Length; k++)
            {
                double x = origindataTransformed[k] * Math.Cos((45 + 0.333 * k) * Math.PI / 180);
                double y = origindataTransformed[k] * Math.Sin((45 + 0.333 * k) * Math.PI / 180);
                Log.WriteCredence(x + " " + y);
                //listy.Add(y);
                //listx.Add(x);
            }
        }
    }
}
