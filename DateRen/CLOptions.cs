using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DateRen
{
    public class CLOptions
    {
        enum NextOption { StartNum, None };
        enum NextArgs { None, Wildcard, Last };

        public string Usage { get { return GetUsage(); } }
        public bool Help { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }

        public bool UseCreateTime { get; set; }
        public bool DestroyOriginalFilename { get; set; }
        public int StartNumber { get; set; }

        public string Wildcard { get; set; }

        static string Version()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.ProductVersion;
        }

        public static string GetUsage()
        {
            return string.Format("DateRen, version {0}\r\n\r\n" +
            "Usage:    DateRen.exe [options] [wildcard]\r\n\r\n" +
            "Options:  -c        Use creation time instead of modification time\r\n" +
            "          -f        Do not leave the original filename as a suffix\r\n" +
            "          -s n      Start numbering at 'n' instead of one\r\n" +
            ""
            , Version());
        }

        public bool HasErrors { get { return Errors.Count > 0; } }
        public bool HasWarnings { get { return Warnings.Count > 0; } }

        private long GetLong(string strValue)
        {
            long lVal;
            if (Int64.TryParse(strValue, out lVal))
                return lVal;

            if (!strValue.StartsWith("0x") && !strValue.StartsWith("-0x"))
            {
                Warnings.Add(String.Format("Could not interpret \"{0}\" as an integer.", strValue));
                return 0;
            }

            int iStart = 2;
            int iMultiply = 1;

            if (strValue[0] == '-')
            {
                iStart = 3;
                iMultiply = -1;
            }

            try
            {
                return Convert.ToInt64(strValue.Substring(iStart), 16) * iMultiply;
            }
            catch (Exception ex)
            {
                Warnings.Add(String.Format("Could not interpret \"{0}\" as an integer, Exception: {1}", strValue, ex.Message));
            }
            return 0;
        }

        public CLOptions(string[] args, bool bSkipZeroArg = false)
        {
            Help = false;
            Errors = new List<string>();
            Warnings = new List<string>();

            Wildcard = "*.*";
            UseCreateTime = false;
            StartNumber = 1;

            NextOption nextOption = NextOption.None;
            NextArgs nextArg = NextArgs.Wildcard;

            foreach (string strArg in args)
            {
                if (bSkipZeroArg)
                {
                    bSkipZeroArg = false;
                    continue;
                }

                switch (nextOption)
                {
                    case NextOption.None:
                        if (strArg[0] == '-' || strArg[0] == '/')
                        {
                            // Arguments passed on the command line with a leading hyphen or slash
                            for (int i = 1; i < strArg.Length; i++)
                            {
                                switch (strArg[i])
                                {
                                    case 'h':
                                    case 'H':
                                    case '?':
                                        Help = true;
                                        break;

                                    case 'c':
                                    case 'C':
                                        UseCreateTime = true;
                                        break;

                                    case 'f':
                                    case 'F':
                                        DestroyOriginalFilename = true;
                                        break;

                                    case 's':
                                    case 'S':
                                        nextOption = NextOption.StartNum;
                                        break;

                                    case '-':
                                        // Options starting with --
                                        switch (strArg.Substring(i + 1))
                                        {
                                            case "help":
                                                Help = true;
                                                break;
                                            default:
                                                Warnings.Add(String.Format("Ignoring unknown option '{0}'", strArg.Substring(i + 1)));
                                                break;
                                        }
                                        i = strArg.Length;
                                        break;
                                    default:
                                        Warnings.Add(String.Format("Ignoring unknown option '{0}'", strArg[i]));
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // Arguments passed on the command line without a leading hyphen
                            switch (nextArg)
                            {
                                case NextArgs.Wildcard:
                                    Wildcard = strArg;
                                    nextArg++;
                                    break;
                                default:
                                    Warnings.Add(String.Format("Ignoring extra command line argument '{0}'", strArg));
                                    break;
                            }
                        }
                        break;

                    case NextOption.StartNum:
                        StartNumber = (int) GetLong(strArg);
                        break;

                    default:
                        Errors.Add("Unknown next option");
                        break;
                }
            }
        }
    }
}
