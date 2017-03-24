using BWReport.DAL.Entities;
using CommonUtil.DBSessionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using CommonUtil.DAO;
using CommonUtil.Entities;

namespace BWReport.DAL.DAO
{
    public class YearlyPerformanceTargetDAO : BaseDAO<YearlyPerformanceTarget, int>
    {
        public IList<LGAGroupedYearlyPerformanceTarget> GenerateYearlyTargetGroupedByLGA(int fYear)
        {

            ICriteria criteria = BuildSession()
                .CreateCriteria<YearlyPerformanceTarget>("ypt").Add(Restrictions.Eq("FiscalYear", fYear))
                .CreateCriteria("HealthFacilty")
                .CreateCriteria("LGA", "l", JoinType.InnerJoin);

            criteria.SetProjection(
                Projections.Alias(Projections.GroupProperty("l.lga_name"), "lga_name"),
                  Projections.Alias(Projections.GroupProperty("l.lga_code"), "lga_code"),
                Projections.Alias(Projections.Sum("ypt.HTC_TST"), "HTC_Target"),
                Projections.Alias(Projections.Sum("ypt.HTC_TST_POS"), "HTC_TST_POS_Target"),
                Projections.Alias(Projections.Sum("ypt.Tx_NEW"), "Tx_New_Target")
                );
            return criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(LGAGroupedYearlyPerformanceTarget))).List<LGAGroupedYearlyPerformanceTarget>();
        }



        public YearlyPerformanceTarget GetTargetByYear(int Year, int faciltyId)// string facilityCode = null)
        {
            YearlyPerformanceTarget result = null;
            ISession session = BuildSession();
            
            ICriteria criteria = session.CreateCriteria<YearlyPerformanceTarget>();
            criteria.Add(Restrictions.Eq("FiscalYear", Year))
            .Add(Restrictions.Eq("HealthFacilty.Id", faciltyId));
            
            //if (!string.IsNullOrEmpty(facilityCode))
            //{
            //    criteria.CreateAlias("HealthFacilty", "hf", JoinType.InnerJoin)
            //    .Add(Restrictions.Eq("FacilityCode", facilityCode));
            //}

            result = criteria.UniqueResult<YearlyPerformanceTarget>();

            return result;
        }


        public bool SaveBatchFromCSV(Stream csvFile, int Year, out string wrongEntries)
        {
            var Ips = new OrganizationDAO().RetrieveAll();
            var lgas = new LGADao().RetrieveAll().GroupBy(x => x.State.state_code);

            StringBuilder sb = new StringBuilder();

            wrongEntries = "";
            var hfDAO = new HealthFacilityDAO();
            var existingFacilities = hfDAO.RetrieveAll()
                .ToDictionary(x => x.FacilityCode);
                //.ToDictionary(x => x.Name);

            List<YearlyPerformanceTarget> targets = new List<YearlyPerformanceTarget>();

            string fileContent = "";

            using (StreamReader reader = new StreamReader(csvFile))
            {
                reader.ReadLine(); //ignore the first line
                fileContent = reader.ReadToEnd();
            }

            string[] theLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in theLines)
            {
                string[] entries = item.Split(',');

                try
                {
                    string healthFacilityCode = entries[1]; //entries[0];
                    if (!string.IsNullOrEmpty(healthFacilityCode))
                    {
                        HealthFacility facility = null;
                        existingFacilities.TryGetValue(healthFacilityCode, out facility);

                        int HTC_TST = 0;
                        int HTC_TST_POS = 0;
                        int Tx_NEW = 0;

                        int.TryParse(entries[2], out HTC_TST);
                        int.TryParse(entries[3], out HTC_TST_POS);
                        int.TryParse(entries[4], out Tx_NEW);

                        if (facility != null)
                        {                
                            if(targets.FirstOrDefault(x=>x.HealthFacilty.FacilityCode == facility.FacilityCode) == null)
                            {
                                targets.Add(new YearlyPerformanceTarget
                                {
                                    Tx_NEW = Tx_NEW,
                                    FiscalYear = Year,
                                    HealthFacilty = facility,
                                    HTC_TST = HTC_TST,
                                    HTC_TST_POS = HTC_TST_POS
                                });
                            }           
                            
                        }
                        else
                        {
                            sb.AppendLine(item + ", unknown facility" + ", <br />");
                        }
                    }
                    else
                    {
                        sb.AppendLine(item + ", unknown facility" + ", <br />");
                    }                    
                }
                catch (Exception ex)
                {
                    sb.AppendLine(item + "," + ex.Message + ",<br />");
                    RollbackChanges();
                }
            }
            wrongEntries = sb.ToString();
            var result = BulkInsert(targets);            
            return result;
        }

        public bool BulkInsert(List<YearlyPerformanceTarget> Targets)
        {
            string tableName = "bwr_YearlyPerformanceTarget";

            var dt = new DataTable(tableName); 
            dt.Columns.Add(new DataColumn("FiscalYear", typeof(int))); 
            dt.Columns.Add(new DataColumn("HTC_TST", typeof(int)));
            dt.Columns.Add(new DataColumn("HTC_TST_POS", typeof(int)));
            dt.Columns.Add(new DataColumn("Tx_NEW", typeof(int)));
            dt.Columns.Add(new DataColumn("HealthFaciltyId", typeof(int))); 


            foreach (var tx in Targets)
            {
                if (GetTargetByYear(tx.FiscalYear, tx.HealthFacilty.Id) != null)
                    continue;

                var row = dt.NewRow();
                  
                row["FiscalYear"] = GetDBValue(tx.FiscalYear);
                row["HTC_TST"] = GetDBValue(tx.HTC_TST);
                row["HTC_TST_POS"] = GetDBValue(tx.HTC_TST_POS);
                row["Tx_NEW"] = GetDBValue(tx.Tx_NEW);
                row["HealthFaciltyId"] = GetDBValue(tx.HealthFacilty.Id); 

                dt.Rows.Add(row);
            }
            DirectDBPost(dt, tableName);
            return true; 
        }
    }
}
