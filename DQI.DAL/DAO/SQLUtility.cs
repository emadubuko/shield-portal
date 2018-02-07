using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Engine;
using System.Data;
using System.Data.SqlClient;

namespace DQI.DAL.DAO
{
    public class SQLUtility
    {
        public static DataTable GetSitesForDQI(string period, string IP="")
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "[sp_get_sites_with_DQI_issues]";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@period", period);
            cmd.Parameters.AddWithValue("@IP", IP);
            
            var dataTable = GetDatable(cmd);

            return dataTable;
        }



        public static DataTable GetDatable(SqlCommand command)
        {
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;
            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                command.Connection = (SqlConnection)connection;
                var dataTable = new DataTable();
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                SqlDataAdapter da = new SqlDataAdapter(command);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
                return dataTable;
            }
        }
    }
}
