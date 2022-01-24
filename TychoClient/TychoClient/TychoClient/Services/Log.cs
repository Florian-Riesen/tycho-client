using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TychoClient.Services
{
    public class Log
    {
        private static Log _instance;

        public static Log Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new Log();
                return _instance;
            }
        }


        public FileInfo LogFile { get; set; }


        public static void Line(string message)
        {
            Trace.WriteLine(message);
            Debug.WriteLine(message);
            if (!(Instance.LogFile is null))
                Instance.WriteToFile(message);
        }

        private void WriteToFile(string message)
        {
            using (var writer = LogFile.AppendText())
            {
                writer.WriteLine(message);
            }
        }
    }
}
