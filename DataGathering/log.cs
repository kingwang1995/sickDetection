using System.Collections.Generic;

namespace DataGathering
{
    public static class Log
    {
        private static LogManager logManager;
        static Log()
        {
            LogManager.Init();
            logManager = new LogManager();
        }
        /// <summary>
        /// 写文件类型
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logFile"></param>
        /// <param name="msg"></param>
        public static void WriteLog(LogType logType, LogFile logFile, string msg)
        {
            try
            {
                logManager.WriteLog(logType, logFile, msg);
            }
            catch
            {

            }
        }
        /// <summary>
        /// 文件类型为null
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="msg"></param>
        public static void WriteLog(LogType logType, string msg)
        {
            try
            {
                logManager.WriteLog(logType, string.Empty, msg);
            }
            catch
            {

            }
        }
        /// <summary>
        /// 数据库异常的标志位
        /// 0表示update，1表示insert
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="status"></param>
        /// <param name="msg"></param>
        public static void WriteLog(LogType logType, string status, string msg)
        {
            try
            {
                logManager.WriteLog(logType, status, msg);
            }
            catch
            {

            }
        }
        /// <summary>
        /// 读日志文件
        /// </summary>
        /// <param name="sqlStatus"></param>
        /// <param name="sqlMsg"></param>
        /// <param name="count"></param>      
        public static List<string> ReadLog()
        {
            List<string> digitList = null;
            try
            {
                digitList = logManager.ReadLog();
            }
            catch { }
            return digitList;
        }

        public static void WriteCredence(string msg)
        {
            try
            {
                logManager.WriteCredence(msg);
            }
            catch { }
        }

        public static void WriteData(string name, string msg)
        {
            try
            {
                logManager.WriteData(name, msg);
            }
            catch { }
        }
    }
}
