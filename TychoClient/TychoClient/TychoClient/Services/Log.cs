using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TychoClient.Services
{
    public class LogService
    {
        private static LogService _instance;
        private static readonly bool LogVerbose = true;

        public static LogService Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new LogService();
                    Line(_instance, "Logging to file: " + _instance.LogFilePath);
                }

                return _instance;
            }
        }


        public string LogFilePath { get; set; }

        public static void Line(object caller, string message, bool isLineVerbose = true)
        {
            if (!LogVerbose && isLineVerbose)
                return;
            message = $"{Now()} {caller.GetType().Name}: {message}";
            Trace.WriteLine(message);
            if (!(Instance.LogFilePath is null))
                Instance.WriteToFile(message);
        }

        private LogService()
        {
            LogFilePath = $"/storage/emulated/0/Android/data/com.companyname.TychoClient/files/Log_{DateTime.Now.ToString("dd.MM.yyyy_HH:mm:ss:FFF")}.txt";
            File.WriteAllText(LogFilePath, "Beginning log on " + DateTime.Now.ToString("dd.MM. HH:mm:ss:FFF") + Environment.NewLine);
        }

        private static string Now() => DateTime.Now.ToString("HH:mm:ss:FFF");

        private void WriteToFile(string message)
        {
            using (var writer = File.AppendText(LogFilePath))
            {
                writer.WriteLine(message);
            }
        }
    }

    public static class LogExtension
    {
        public static void Log(this object caller, string message, bool isLineVerbose = true)
        {
            LogService.Line(caller, message, isLineVerbose);
        }
    }
}
