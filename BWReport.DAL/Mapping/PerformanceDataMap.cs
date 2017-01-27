using BWReport.DAL.Entities;
using FluentNHibernate.Mapping;

namespace BWReport.DAL.Mapping
{
    public class PerformanceDataMap : ClassMap<PerformanceData>
    {
        public PerformanceDataMap()
        {
            Table("bwr_PerformanceData");
            Id(x => x.Id);
            Map(x => x.HTC_TST);
            Map(x => x.HTC_TST_POS);
            Map(x => x.Tx_NEW);
            Map(x => x.ReportPeriod);
            Map(x => x.FY);
            //Map(x => x.ReportPeriodFrom);
            //Map(x => x.ReportPeriodTo);
            References(x => x.HealthFacility).Column("HealthFacilityId");
            References(x => x.ReportUpload).Column("ReportId");
             
        }
    }
}
