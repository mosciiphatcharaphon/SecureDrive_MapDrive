using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDAVClient.Helpers
{
    public class Log
    {
        private static System.IO.StreamWriter systemLogWriter;
        public static void DataLog(string message)
        {
            try
            {
                string todayFolder = DateTime.Today.Date.ToString("yyyy'-'MM'-'dd");
                string logLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MapSecureDrive", "Logs");
                if (!Directory.Exists(logLocation))
                    Directory.CreateDirectory(logLocation);
                string LogFullPath = logLocation + "\\" + todayFolder;
                if (!Directory.Exists(LogFullPath))
                {
                    Directory.CreateDirectory(LogFullPath);
                    systemLogWriter = System.IO.File.AppendText(LogFullPath + "\\logClient.log");
                    systemLogWriter.AutoFlush = true;

                }
                if (systemLogWriter == null)
                {
                    systemLogWriter = System.IO.File.AppendText(LogFullPath + "\\logClient.log");
                    systemLogWriter.AutoFlush = true;
                }

                if (systemLogWriter != null)
                {
                    string d = Convert.ToString(DateTime.Now);
                    Console.WriteLine(d + "::" + message);
                    systemLogWriter.WriteLine(d + "::" + message);
                    systemLogWriter.Flush();
                }
            }
            catch { }
        }
    }
}
