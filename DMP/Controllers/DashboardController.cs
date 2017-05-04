using DQA.DAL.Business;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;

namespace ShieldPortal.Controllers
{
    public class DashboardController : ApiController
    {
        //[Route("api/Dashboard/GetIpCounts/{partner_id}/{reporting_period}")]
        public DataSet GetIpCounts(string reporting_period, int year, int partner_id = 0)
        {
            //TODO: seperate period into year and quarter
            var cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@period", reporting_period);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.CommandType = CommandType.StoredProcedure;
                        
            if (partner_id != 0)
            {
                cmd.CommandText = "sp_get_dqa_dashboard_details";
                cmd.Parameters.AddWithValue("@ip_id", partner_id);
            }
            else
            {
                cmd.CommandText = "sp_get_dqa_dashboard_details_cdc";
            }
            return Utility.GetDataSet(cmd);
        }


        //[Route("api/Dashboard/GetHome/{reporting_period}")]
        //public DataSet GetHome(string reporting_period)
        //{
        //    var cmd = new SqlCommand();
        //    cmd.CommandText = "sp_get_dqa_dashboard_details_cdc";
        //    cmd.Parameters.AddWithValue("@period", reporting_period);
        //    cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //    return Utility.GetDataSet(cmd);
        //}


    }
}
