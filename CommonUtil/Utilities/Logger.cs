using System;
using System.Text;

namespace CommonUtil.Utilities
{
    public class Logger
    {
        static string filename = string.Format("{0}//shieldportalErrors_{1:dd-MMM-yyyy}.txt", System.Configuration.ConfigurationManager.AppSettings["Logfile"], DateTime.Now);
        

        public static void LogInfo(string MethodName, object message)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss"));
                sb.AppendLine("caller: " + MethodName + "\n: " + message);

                using (System.IO.StreamWriter str = new System.IO.StreamWriter(filename, true))
                {
                    str.WriteLine(sb.ToString());
                }                 
            }
            catch { }
        }

        public static void LogError(Exception ex)
        {
            try
            {
                string message = GetExceptionMessages(ex);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Date and Time: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss"));
                sb.AppendLine("ErrorMessage: \n " + message);

                using (System.IO.StreamWriter str = new System.IO.StreamWriter(filename, true))
                {
                    str.WriteLine(sb.ToString());
                } 
            }
            catch { }
        }

        static string GetExceptionMessages(Exception ex)
        {
            string ret = string.Empty;
            if (ex != null)
            {
                ret = ex.Message;
                if (ex.InnerException != null)
                    ret = ret + "\n" + GetExceptionMessages(ex.InnerException);
            }
            return ret;
        }
    }
}
