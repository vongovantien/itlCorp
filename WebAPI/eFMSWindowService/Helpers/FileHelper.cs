using System;
using System.Configuration;
using System.IO;

namespace eFMSWindowService.Helpers
{
    public static class FileHelper
    {
        public static void WriteToFile(string logName, string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + logName + "_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);

                    if (ConfigurationManager.AppSettings["MonitorFileLogs"] =="1")
                        WriteLogs(logName, Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                    if (ConfigurationManager.AppSettings["MonitorFileLogs"] == "1")
                        WriteLogs(logName, Message);
                }
            }
        }

        public static void WriteLogs(string logName, string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + "WriteLogs" + ".log";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("<<<------------------------------------------------------------------------->>>");
                    sw.WriteLine("[{0} {1}][FILE LOGS]:{2}" , DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(),logName);
                    sw.WriteLine("[MSG] {0}",Message);
                    sw.WriteLine("<<<------------------------------------------------------------------------->>>");
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {

                    sw.WriteLine("<<<------------------------------------------------------------------------->>>");
                    sw.WriteLine("[{0} {1}][FILE LOGS]:{2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), logName);
                    sw.WriteLine("[MSG] {0}", Message);
                    sw.WriteLine("<<<------------------------------------------------------------------------->>>");

                }
            }
        }
    }
}
