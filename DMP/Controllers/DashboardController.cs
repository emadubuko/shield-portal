
using DQA.DAL.Business;
using DQA.DAL.Model;
using DQA.ViewModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;

namespace ShieldPortal.Controllers
{
    public class DashboardController : ApiController
    {

        // GET: api/Dashboard/5
        [Route("api/Dashboard/GetIpCounts/{partner_id}/{reporting_period}")]
        public DataSet GetIpCounts(int partner_id, string reporting_period)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_get_dqa_dashboard_details";
            cmd.Parameters.AddWithValue("@ip_id", partner_id);
            cmd.Parameters.AddWithValue("@period", reporting_period);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            return Utility.GetDataSet(cmd);
          
        }
      
     
    }
}
