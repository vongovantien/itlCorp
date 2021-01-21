using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace eFMS.API.Common.Helpers
{
    public class LogHelper
    {
        private readonly string exePath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
        private readonly string logName = "eFMS-LOG";

        public LogHelper() { }

        public LogHelper(string logMessage)
        {
            LogWrite(logName, logMessage);
        }

        public LogHelper(string _logName, string logMessage)
        {
            LogWrite(_logName, logMessage);
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
            }
        }

        public void LogWrite(string logName, string logMessage)
        {
            try
            {
                if (!Directory.Exists(exePath))
                {
                    Directory.CreateDirectory(exePath);
                }
                string filepath = exePath + "\\" + logName + "_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!System.IO.File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = System.IO.File.CreateText(filepath))
                    {
                        sw.WriteLine(logMessage);
                    }
                }
                else
                {
                    using (StreamWriter sw = System.IO.File.AppendText(filepath))
                    {
                        sw.WriteLine(logMessage);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
