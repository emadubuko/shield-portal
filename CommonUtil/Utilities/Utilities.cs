using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonUtil.Utilities
{
    public class Utilities
    {
        public static string PasCaseConversion(string PascalWord)
        {
            return Regex.Replace(PascalWord, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
            //System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("([A-Z]+[a-z]+)");
            //string result = _regex.Replace(PascalWord, m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ");
            //return result;
        }

       public static bool IsAnyNullOrEmpty(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject, null);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string PasCaseConversion(object PascalWord)
        {
            if (PascalWord != null && !string.IsNullOrEmpty(Convert.ToString(PascalWord)))
                return PasCaseConversion(Convert.ToString(PascalWord));
            else
                return "";
        }

        public static string Sha256_Hash(string secret)
        {
            SHA256Managed crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(secret), 0, Encoding.UTF8.GetByteCount(secret));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
         

        public static Dictionary<string, string> RetrieveDashboard(string role, string IPShortname, List<string> dashboardtype =null)
        {
            Dictionary<string, string> iframe = new Dictionary<string, string>();
            string script = "";
            if(dashboardtype != null)
            {
                script = string.Format("SELECT [DashboardFrame], [DashBoardType] FROM [PowerBiDashboard] where DashBoardType in '({0})' and rolename='{1}' and dashboardscope='{2}'", string.Join(",", dashboardtype), role, IPShortname);
            }
            else
            {
                script = string.Format("SELECT [DashboardFrame], [DashBoardType] FROM [PowerBiDashboard] where rolename='{0}' and dashboardscope='{1}'", role, IPShortname);
            }
            
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(script, s);
                SqlDataReader reader = null;
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    iframe.Add(reader[1] as string, reader[0] as string);
                }
                reader.Dispose();
            }
            return iframe;
        }


    }
}
