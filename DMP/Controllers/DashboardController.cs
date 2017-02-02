
using DQA.DAL.Business;
using DQA.DAL.Model;
using DQA.ViewModel;
using System.Collections.Generic;
using System.Web.Http;

namespace ShieldPortal.Controllers
{
    public class DashboardController : ApiController
    {

        // GET: api/Dashboard/5
        [Route("api/Dashboard/GetIpCounts/{partner_id}/{reporting_period}")]
        public string GetIpCounts(int partner_id, string reporting_period)
        {

            return Utility.GetIpFacilitycount(partner_id) + "|" +Utility.GetIpSubmitted(partner_id, reporting_period)+"|"+Utility.GetIpLGACount(partner_id) +"|"+Utility.GetIpStateCount(partner_id);
        }

        [Route("api/Dashboard/GetStateSummary/{state_id}/{reporting_period}")]
        public List<StateSummary> GetStateSummary(int state_id,string reporting_period)
        {
            return Utility.GetStateIpStateSummary(state_id, reporting_period);
        }


        [Route("api/Dashboard/GetPendingFacilities/{partner_id}/{reporting_period}")]
        public List<Facility> GetPendingFacilities(int partner_id, string reporting_period)
        {
            var pending_facs= Utility.GetPendingFacilities(partner_id, reporting_period);
            var facilities = new List<Facility>();
            foreach(var pending_fac in pending_facs)
            {
                facilities.Add(new Facility(pending_fac));
            }
            return facilities;
        }
      
     
    }
}
