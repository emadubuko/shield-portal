using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace CommonUtil.Utilities
{
   public class ExcelHelper
    {
        public static Dictionary<string, int> GenerateIndexedPeriods()
        {
            Dictionary<string, int> indexedPeriod = new Dictionary<string, int>();
            string script = "Select ReportPeriod, ExcelColumn from ReportPeriodToDateMapper";

            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(script, s);
                SqlDataReader reader = null;
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    indexedPeriod.Add(reader[0] as string, Convert.ToInt32(reader[1]));
                }
                reader.Dispose();
            }

            return indexedPeriod;
        }
    }
}
