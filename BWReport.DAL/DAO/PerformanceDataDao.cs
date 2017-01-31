using BWReport.DAL.Entities;
using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BWReport.DAL.DAO
{
    public class PerformanceDataDao : BaseDAO<PerformanceData, long>
    {
        public List<PerformanceData> RetrieveByOrganizationShortName(int fYear, string OrgShortName)
        {
            ICriteria criteria = BuildSession()
               .CreateCriteria<PerformanceData>("pd").Add(Restrictions.Eq("FY", fYear))
               .CreateCriteria("HealthFacility")
               .CreateCriteria("Organization", "org", NHibernate.SqlCommand.JoinType.InnerJoin)
               .Add(Restrictions.Eq("ShortName", OrgShortName));
            List<PerformanceData> data = criteria.List<PerformanceData>() as List<PerformanceData>;
            return data;

        }

        public IList<LGALevelAchievementPerTarget> GenerateLGALevelAchievementPerTarget(int  fYear)
        {
            
            ICriteria criteria = BuildSession()
                .CreateCriteria<PerformanceData>("pd").Add(Restrictions.Eq("FY", fYear))
                .CreateCriteria("HealthFacility")
                .CreateCriteria("LGA", "l", NHibernate.SqlCommand.JoinType.InnerJoin);


            criteria.SetProjection(
                Projections.Alias(Projections.GroupProperty("l.lga_name"), "lga_name"),
                 Projections.Alias(Projections.GroupProperty("l.lga_code"), "lga_code"),
                Projections.Alias(Projections.Sum("pd.HTC_TST"), "TOTAL_HTC_TST"),
                Projections.Alias(Projections.Sum("pd.HTC_TST_POS"), "TOTAL_HTC_TST_POS"),
                Projections.Alias(Projections.Sum("pd.Tx_NEW"), "Total_Tx_New")
                );
            IList<LGALevelAchievementPerTarget> lgaMeasures = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(LGALevelAchievementPerTarget))).List<LGALevelAchievementPerTarget>();

            var yearlyTargetGroupedByLGA = new YearlyPerformanceTargetDAO().GenerateYearlyTargetGroupedByLGA(fYear).ToDictionary(x => x.lga_code);
            foreach (var item in lgaMeasures)
            {
                LGAGroupedYearlyPerformanceTarget target = null;
                yearlyTargetGroupedByLGA.TryGetValue(item.lga_code, out target);

                if (target == null)
                    continue;

                item.HTC_TST_Target = target.HTC_Target;
                item.HTC_TST_POS_Target = target.HTC_TST_POS_Target;
                item.Tx_New_Target = target.Tx_New_Target;
            }

            return lgaMeasures;
        }

        public IList<Facility_Community_Postivity> ComputePositivityRateByFacilityType(int fYear)
        {
            ICriteria criteria = BuildSession()
                .CreateCriteria<PerformanceData>("pd").Add(Restrictions.Eq("FY", fYear))
                .CreateCriteria("HealthFacility","hf")
                .CreateCriteria("LGA", "l", NHibernate.SqlCommand.JoinType.InnerJoin);


            criteria.SetProjection(
                Projections.Alias(Projections.GroupProperty("l.lga_name"), "lga_name"),
                 Projections.Alias(Projections.GroupProperty("l.lga_code"), "lga_code"),
                Projections.Alias(Projections.Sum("pd.HTC_TST"), "Tested"),
                Projections.Alias(Projections.Sum("pd.HTC_TST_POS"), "Positive"),
                Projections.Alias(Projections.GroupProperty("hf.OrganizationType"), "FacilityType")
                );

            IList<Facility_Community_Postivity> rates = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(Facility_Community_Postivity))).List<Facility_Community_Postivity>();
            return rates;
        }


        public IList<IPUploadReport> GenerateIPUploadReports(int fYear)
        {
            ICriteria criteria = BuildSession()
                .CreateCriteria<PerformanceData>("pd").Add(Restrictions.Eq("FY", fYear))
                .CreateCriteria("HealthFacility", "hf")
                .CreateCriteria("Organization", "org", NHibernate.SqlCommand.JoinType.InnerJoin);
            
            criteria.SetProjection(
                Projections.Alias(Projections.GroupProperty("org.Name"), "IPName"),
                 Projections.Alias(Projections.GroupProperty("pd.ReportPeriod"), "ReportPeriod")
                );

            IList<IPUploadReport> reports = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(IPUploadReport))).List<IPUploadReport>();
            return reports;
        }


        public bool BulkInsert(List<PerformanceData> TargetMeasures)
        {

            string tableName = "bwr_PerformanceData";

            var dt = new DataTable(tableName);
            //dt.Columns.Add(new DataColumn("ReportPeriodFrom", typeof(DateTime)));
            //dt.Columns.Add(new DataColumn("ReportPeriodTo", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("ReportPeriod", typeof(string)));
            dt.Columns.Add(new DataColumn("FY", typeof(int)));
            dt.Columns.Add(new DataColumn("HTC_TST", typeof(int)));
            dt.Columns.Add(new DataColumn("HTC_TST_POS", typeof(int)));
            dt.Columns.Add(new DataColumn("Tx_NEW", typeof(int)));
            dt.Columns.Add(new DataColumn("HealthFacilityId", typeof(int)));
            dt.Columns.Add(new DataColumn("ReportId", typeof(int)));
            

            foreach (var tx in TargetMeasures)
            {
                var row = dt.NewRow();

                //row["ReportPeriodFrom"] = GetDBValue(tx.ReportPeriodFrom);
                //row["ReportPeriodTo"] = GetDBValue(tx.ReportPeriodTo);
                row["ReportPeriod"] = GetDBValue(tx.ReportPeriod);
                row["FY"] = GetDBValue(tx.FY);
                row["HTC_TST"] = GetDBValue(tx.HTC_TST);
                row["HTC_TST_POS"] = GetDBValue(tx.HTC_TST_POS);
                row["Tx_NEW"] = GetDBValue(tx.Tx_NEW);
                row["HealthFacilityId"] = GetDBValue(tx.HealthFacility.Id);
                row["ReportId"] = GetDBValue(tx.ReportUpload.Id);
                
                dt.Rows.Add(row);
            }
            DirectDBPost(dt, tableName);
            return true; //if we get this far, everything is ok
        }

    }
}
