using System;
using System.Collections.Generic;
using System.IO;

namespace DateRen
{
    internal class DateRenamer
    {
        private CLOptions options;
        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public event LogEventHandler Log;

        public DateRenamer(CLOptions options)
        {
            this.options = options;
        }

        internal void Run()
        {
            string dir = ".";
            if (options.Wildcard.Contains("\\"))
                dir = Path.GetDirectoryName(options.Wildcard);
            string spec = "*.*";
            if (Directory.Exists(options.Wildcard))
                dir = options.Wildcard;
            else
                spec = Path.GetFileName(options.Wildcard);
            string[] files = Directory.GetFiles(dir, spec);
            Dictionary<int, int> dictIndices = new Dictionary<int, int>();
            foreach (string file in files)
            {
                string strFileName = Path.GetFileName(file);
                DateTime dt = options.UseCreateTime  ? File.GetCreationTime(file) : File.GetLastWriteTime(file);
                int iDateIndex = dt.Year * 366 + dt.DayOfYear;
                if (!dictIndices.ContainsKey(iDateIndex))
                    dictIndices.Add(iDateIndex, options.StartNumber);
                string strFormattedDate = String.Format("{0:D4}-{1:D2}-{2:D2}-{3:D3}", dt.Year, dt.Month, dt.Day, dictIndices[iDateIndex]);
                dictIndices[iDateIndex] = dictIndices[iDateIndex] + 1;
                string strNewName = file;
                if (!strFileName.StartsWith(strFormattedDate))
                {
                    strNewName = String.Format("{0}-{1}", strFormattedDate, strFileName);
                    if (options.DestroyOriginalFilename)
                        strNewName = String.Format("{0}{1}", strFormattedDate, Path.GetExtension(strFileName));
                    Log?.Invoke(this, new LogEventArgs(String.Format("{0} => {1}", strFileName, strNewName)));
                    File.Move(strFileName, strNewName);
                }
            }
        }
    }

    public class LogEventArgs : EventArgs
    {
        public string Message;

        public LogEventArgs(string message)
        {
            Message = message;
        }
    }
}