using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataGathering
{
    /// <summary>
    /// 日志管理
    /// </summary>
    public class LogManager
    {
        private string filepath = "log";
        //public static string logAddressStr = "filepath";
        //public static string logAddress;
        DateTime time;                                        //文件创建时间
        private string logFileExtName = "log";     //日志文件扩展名
        private Encoding logFileEncoding = Encoding.UTF8;   //日志文件编码格式
        private string logFileName = string.Empty;  //日志文件名
        private string logPath = "";            //日志文件路径
        private bool writeLogTime = true;     //log文件是否写时间
        private bool writeStatus = false;        //是否写入标志位
        private static object obj = new object();
        /// <summary>
        /// 配置文件初始化
        /// </summary>
        public static void Init()
        {
            //try
            //{
            //    logAddress = ConfigurationManager.AppSettings[logAddressStr];
            //}
            //catch
            //{
            //    Console.WriteLine("配置文件有误");
            //}
        }
        /// <summary>
        /// 日志文件路径
        /// </summary>
        public string CreateLogPath()
        {
            if (!Directory.Exists(filepath))
            {
                try
                {
                    Directory.CreateDirectory(filepath);
                }
                catch
                {
                    Console.WriteLine("创建文件路径失败");
                }
            }
            if (logPath == null || logPath == string.Empty || time.ToString("yyyy-MM-dd") != System.DateTime.Now.ToString("yyyy-MM-dd"))
            {
                try
                {
                    time = System.DateTime.Now;
                    logPath = System.IO.Path.Combine(filepath, time.ToString("yyyy-MM-dd"));
                }
                catch
                {
                    Console.WriteLine("路径合成失败");
                }
            }
            if (!logPath.EndsWith(@"\"))
            {
                logPath += @"\";
            }
            if (!Directory.Exists(logPath))
            {
                try
                {
                    Directory.CreateDirectory(logPath);
                }
                catch
                {
                    Console.WriteLine("创建文件路径失败");
                }
            }
            return logPath;
        }
        /// <summary>
        /// 写日志
        /// </summary>
        public void WriteLog(LogType logType, string logFile, string msg)
        {
            CreateLogPath();
            lock (obj)
            {
                try
                {
                    //创建log文件
                    logFileName = string.Format("{0}{1}.{2}", logPath, logType, this.logFileExtName);
                    using (StreamWriter sw = new StreamWriter(logFileName, true, logFileEncoding))
                    {
                        //是否写时间
                        if (logType == LogType.PROCESS)
                        {
                            writeLogTime = true;
                        }
                        else
                        {
                            writeLogTime = false;
                        }
                        ////sql类型
                        //if (logType == LogType.DATABASE)
                        //{
                        //    writeStatus = true;
                        //}
                        //else
                        //{
                        //    writeStatus = false;
                        //}
                        if (writeLogTime)
                        {
                            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:") + logFile + ":" + msg);
                        }
                        else
                        {
                            sw.WriteLine(logFile + "" + msg);
                        }
                    }
                }
                catch { }
            }
        }
        /// <summary>
        /// log类型不定
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logFile"></param>
        /// <param name="msg"></param>
        public void WriteLog(LogType logType, LogFile logFile, string msg)
        {
            this.WriteLog(logType, logFile.ToString(), msg);
        }
        /// <summary>
        /// log类型为null
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="msg"></param>
        public void WriteLog(LogType logType, string msg)
        {
            this.WriteLog(logType, string.Empty, msg);
        }
        /// <summary>
        /// 读日志文件
        /// </summary>
        /// <param name="sqlStatus"></param>
        /// <param name="sqlMsg"></param>
        public List<string> ReadLog()
        {
            List<string> digitList = new List<string>();
            //string date = System.DateTime.Now.ToString("yyyy-MM-dd");
            //string filePath = logAddress + "\\\\" + date + "\\\\" + "CREDENCES.log";
            string filePath = filepath + "\\\\" + "CREDENCES.log";
            try
            {
                List<string> logLines = null;
                if (File.Exists(filePath))
                {
                    logLines = new List<string>(File.ReadAllLines(filePath));
                    File.Delete(filePath);
                }
                if (logLines != null)
                {
                    for (int i = 0; i < 300; i++)
                    {
                        string logLine = logLines[i];
                        digitList.Add(logLine);
                        logLines.RemoveAt(i);
                    }
                    File.WriteAllLines(filePath, logLines.ToArray());
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return digitList;
        }
        public void WriteCredence(string msg)
        {
            lock (obj)
            {
                try
                {
                    //创建log文件
                    string logname = string.Format("{0}.{1}", "CREDENCES", "txt");
                    logFileName = filepath + "\\" + logname;
                    using (StreamWriter sw = new StreamWriter(logFileName, true, logFileEncoding))
                    {
                        if (msg.Length == 8)
                        {
                            sw.WriteLine(msg);
                        }
                        else
                        {
                            sw.WriteLine(msg);
                        }
                    }
                }
                catch { }
            }
        }

        public void WriteData(string name, string msg)
        {
            CreateLogPath();
            lock (obj)
            {
                try
                {
                    //创建log文件
                    string logname = string.Format("{0}.{1}", name, "txt");
                    //logFileName = filepath + "\\" + logname;
                    logFileName = logPath + "\\" + logname;
                    using (StreamWriter sw = new StreamWriter(logFileName, true, logFileEncoding))
                    {
                        if (msg.Length == 8)
                        {
                            sw.WriteLine(msg);
                        }
                        else
                        {
                            sw.WriteLine(msg);
                        }
                    }
                }
                catch { }
            }
        }




    }
    /// <summary>
    /// log类型
    /// </summary>
    public enum LogFile
    {
        LOG,
        RESET,
        ERROR,
        WARNING,
        INFO,
        ERROR_NUMBERPLATE
    }
    /// <summary>
    /// log文件类型
    /// </summary>
    public enum LogType
    {
        PROCESS,
        CREDENCES
    }
}
