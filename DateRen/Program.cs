using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateRen
{
    class Program
    {
        static void Main(string[] args)
        {
            CLOptions options = new CLOptions(args);
            if (options.Help)
            {
                Console.WriteLine(options.Usage);
                return;
            }
            if (options.HasErrors)
            {
                foreach (String strErr in options.Errors)
                    Console.Error.WriteLine(strErr);
                return;
            }
            foreach (String strWarn in options.Warnings)
                Console.Error.WriteLine(strWarn);
            DateRenamer dr = new DateRenamer(options);
            dr.Log += Logger;
            dr.Run();
        }

        private static void Logger(object sender, LogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
