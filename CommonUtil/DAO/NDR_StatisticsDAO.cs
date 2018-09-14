using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtil.DAO
{
    public class NDR_StatisticsDAO : BaseDAO<NDR_Statistics, int>
    {
        string url = ConfigurationManager.AppSettings["ndr_statistics_url"];

        public async Task RefreshDataFromServer()
        {
            var data = await new Utilities.Utilities().GetDateListRemotely<NDR_Statistics>(url);
            var healthfacilities = new HealthFacilityDAO().RetrieveAll().ToDictionary(c => c.FacilityCode);

            using (var session = BuildSession().SessionFactory.OpenStatelessSession())
            using (var tx = session.BeginTransaction())
            { 
                foreach (var a in data)
                {
                    a.CachedDatetime = DateTime.Now;
                    if(healthfacilities.TryGetValue(a.FacilityCode, out HealthFacility hf))
                    {
                        a.Facility = hf;
                        session.Insert(a);
                    }                   
                }
                tx.Commit();
            } 
        }

        public new async Task<IList<NDR_Statistics>> RetrieveAll()
        {
           var first =  Retrieve(1);
            IList<NDR_Statistics> data = null;
            if (first == null || first.CachedDatetime.Date < DateTime.Today)
            {
                string sql = "truncate table NDR_Statistics";
                RunSQL(sql); 
                await RefreshDataFromServer(); 
            }
            data = base.RetrieveAll();
            return data;
        }

    }
}
