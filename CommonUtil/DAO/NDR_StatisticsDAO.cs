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

        public async Task RefreshDataFromServer()
        {
            string url = ConfigurationManager.AppSettings["ndr_statistics_url"];

            var data = await new Utilities.Utilities().GetDateListRemotely<NDR_Statistics>(url);
            var healthfacilities = new HealthFacilityDAO().RetrieveAll().ToDictionary(c => c.FacilityCode);

            using (var session = BuildSession().SessionFactory.OpenStatelessSession())
            using (var tx = session.BeginTransaction())
            {
                foreach (var a in data)
                {
                    a.CachedDatetime = DateTime.Now;
                    if (healthfacilities.TryGetValue(a.FacilityCode, out HealthFacility hf))
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
            var first = Retrieve(1);
            IList<NDR_Statistics> data = null;
            bool refresh = Convert.ToBoolean(ConfigurationManager.AppSettings["refresh_ndr_statistics"]);

            if (first == null)
            {
                await RefreshDataFromServer();
            }
            else if(refresh && first.CachedDatetime.Date < DateTime.Today)
            {
                string sql = "truncate table NDR_Statistics";
                RunSQL(sql);
                await RefreshDataFromServer();
            }
            data = base.RetrieveAll();
            return data;
        }

        public IEnumerable<NDR_Statistics> RetrievePivotTablesForComparison(List<string> ip, string quarter, List<string> state_code = null, List<string> lga_code = null, List<string> facilityName = null)
        {
            IEnumerable<NDR_Statistics> list = RetrieveAll().Result;

            if (ip != null && ip.Count > 0)
            {
                if (ip.Count() == 1 && string.IsNullOrEmpty(ip.FirstOrDefault()))
                {
                    //do nothing
                }
                else
                    list = list.Where(x => ip.Contains(x.Facility.Organization.ShortName));
            }

            if (state_code != null && state_code.Count > 0)
            {
                list = list.Where(x => state_code.Contains(x.Facility.LGA.State.state_code));
            }
            if (lga_code != null && lga_code.Count > 0)
            {
                list = list.Where(x => lga_code.Contains(x.Facility.LGA.lga_code));
            }
            if (facilityName != null && facilityName.Count > 0)
            {
                list = list.Where(x => facilityName.Contains(x.Facility.Name));
            }

            return list;
        }

        public async Task<IList<NDR_Facilities>> FacilitiesForRADET()
        {
            string facility_url = ConfigurationManager.AppSettings["ndr_facility"];

            var facilities = new BaseDAO<NDR_Facilities, int>().RetrieveAll();

            if (facilities == null || facilities.Count == 0)
            {
                facilities = await new Utilities.Utilities().
               GetDateListRemotely<NDR_Facilities>(facility_url);

                using (var session = BuildSession().SessionFactory.OpenStatelessSession())
                using (var tx = session.BeginTransaction())
                {
                    foreach (var a in facilities)
                    {
                        session.Insert(a);
                    }
                    tx.Commit();
                }
            }
            return facilities;
        }

        
    }
}
