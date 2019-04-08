using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sickTest
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        NetworkStream stream;
        private int bufferSize = 0x8000;
        private string startCmd = "02 73 45 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 31 03";
        private string endCmd = "02 73 45 4E 20 4C 4D 44 73 63 61 6E 64 61 74 61 20 30 03";
        int flag = 0;
        public Form1()
        {
            InitializeComponent();
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse("192.168.1.31"), 2111);
            Console.WriteLine("连接成功");
            stream = tcpClient.GetStream();
        }
        //开始
        private void button1_Click(object sender, EventArgs e)
        {
            //byte[] byteStartCmd = HexStringToBinary(startCmd);
            //string result = HexStringToASCII(byteStartCmd);
            //byte[] b = Encoding.UTF8.GetBytes(result);
            //stream.Write(b, 0, b.Length);
            //byte[] data = new byte[bufferSize];
            //stream.Read(data, 0, bufferSize);
            //stream.Close();
            //string str = HexStringToASCII(data);
            // 初始化一个缓存区
            flag = sendCmd(startCmd);
            byte[] buffer = new byte[bufferSize];
            int read = 0;
            int block;
            Task.Factory.StartNew(() =>
            {
                while ((block = stream.Read(buffer, read, buffer.Length - read)) > 0 && flag != 2)
                {
                    // 重新设定读取位置
                    read += block;

                    // 检查是否到达了缓存的边界，检查是否还有可以读取的信息
                    if (read == buffer.Length)
                    {
                        // 尝试读取一个字节
                        int nextByte = stream.ReadByte();

                        // 读取失败则说明读取完成可以返回结果
                        if (nextByte == -1)
                        {
                            Console.WriteLine("读取失败");
                            //return buffer;
                        }

                        // 调整数组大小准备继续读取
                        byte[] newBuf = new byte[buffer.Length * 2];
                        Array.Copy(buffer, newBuf, buffer.Length);
                        newBuf[read] = (byte)nextByte;

                        // buffer是一个引用（指针），这里意在重新设定buffer指针指向一个更大的内存
                        buffer = newBuf;
                        read++;
                    }
                    Thread.Sleep(1);
                }
            });
            Task.Factory.StartNew(() =>
            {
                string str;
                while (buffer != null && flag != 2)
                {
                    str = System.Text.Encoding.Default.GetString(buffer);
                    Log.WriteLog(LogType.PROCESS, str);
                    //this.Invoke(new Action(() =>
                    //{
                    //    str = System.Text.Encoding.Default.GetString(buffer);
                    //    this.richTextBox1.Text = str;
                    //    Thread.Sleep(1000);
                    //    this.richTextBox1.Clear();
                    //    string[] data = str.Split(' ');
                    //    if (data.Length >= 25) 
                    //    {
                    //        Console.WriteLine(data[25]);
                    //    }
                    //}));
                    Thread.Sleep(1000);
                }
            });
        }
        //结束
        private void button2_Click(object sender, EventArgs e)
        {
            flag = sendCmd(endCmd);
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
    }
}
