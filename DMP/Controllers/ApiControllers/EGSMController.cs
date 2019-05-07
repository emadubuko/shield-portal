using CommonUtil.Utilities;
using MPM.DAL;
using MPM.DAL.DAO;
using Newtonsoft.Json;
using ShieldPortal.Models;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShieldPortal.Controllers.ApiControllers
{
    public class EGSMController : ApiController
    {

        

        [HttpGet]
        public HTSReport getGSMHTSCummulative(string siteName, string reportingPeriod)
        {
            MPMDAO dao = new MPMDAO();
            MPMDataSearchModel searchModel = new MPMDataSearchModel();
            searchModel.facilities = new List<string> { siteName };
            searchModel.ReportPeriod = reportingPeriod;

            //??? ought to be a defined constant
            const string STORED_PROCEDURE = "sp_MPM_HTS_Other_PITC_Completeness_Rate";

            var data = dao.RetriveDataAsDataTables(STORED_PROCEDURE, searchModel);
            List<HTS_Other_PITC_Completeness_Rate> htc_list = Utilities.ConvertToList<HTS_Other_PITC_Completeness_Rate>(data);
            HTSReport response = new HTSReport();
            List<SDPAggregate> sdp_list = new List<SDPAggregate>(); 


            int sum_pos = 0;
            int sum_neg = 0;

            int sum_pos_greater_fifteen = 0;
            int sum_neg_greater_fifteen = 0;

            var list_less_than_fifteen = new List<string> { "<10", "10-14" };
            

            htc_list.ForEach(x =>
            {
                sum_pos +=  Int32.Parse(x.POS);
                sum_neg += Int32.Parse(x.NEG);

                if (!list_less_than_fifteen.Contains(x.AgeGroup))
                {
                    sum_pos_greater_fifteen +=  Int32.Parse(x.POS);
                    sum_neg_greater_fifteen += Int32.Parse(x.NEG);

                }

                var sdp_item = new SDPAggregate();
                sdp_item.SDP_Name = x.SDP;
                sdp_item.yield = Int32.Parse(x.POS);

                sdp_list.Add(sdp_item);
                
            });

            response.HTS_TST = sum_pos + sum_neg;
            if(response.HTS_TST > 0)
            {
                response.HTS_TST_POS = ( (double)(sum_pos_greater_fifteen + sum_neg_greater_fifteen) / (response.HTS_TST));

                var groupResult = sdp_list.GroupBy(a => a.SDP_Name)
               .Select(x => new { Sdp_Name = x.Key, totalYield = (x.Sum(b => b.yield))/sum_pos }).ToList();

                sdp_list.Clear();

                if (groupResult != null)
                {

                    foreach(var item in groupResult)
                    {
                        var sdp_item = new SDPAggregate();

                        sdp_item.SDP_Name = item.Sdp_Name;
                        sdp_item.yield = item.totalYield;
                        sdp_list.Add(sdp_item);
                    }
                }

                response.Yield = JsonConvert.SerializeObject(sdp_list);
            }
           

        return response;

        }
    }
}
