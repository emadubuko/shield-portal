using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Engine;
using System;
using System.Data.SqlClient;
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

        public static string PasCaseConversion(object PascalWord)
        {
            if (PascalWord != null && !string.IsNullOrEmpty(Convert.ToString(PascalWord)))
                return PasCaseConversion(Convert.ToString(PascalWord));
            else
                return "";
        }

        public static string RetrieveBiWeeklyDashboard(string role, string IPShortname)
        {
            string iframe = "";
            string script = string.Format("SELECT [DashboardFrame] FROM [PowerBiDashboard] where rolename='{0}' and dashboardscope='{1}'", role, IPShortname);

            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(script, s);
                SqlDataReader reader = null;
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    iframe = reader[0] as string;
                }
                reader.Dispose();
            }
            return iframe;
        }
    }
}
