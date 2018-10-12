using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{

    public class NDR_StatisticsMap : ClassMap<NDR_Statistics>
    {
        public NDR_StatisticsMap()
        {
            Table("NDR_Statistics");

            Id(i => i.Id);
            References(m => m.Facility).Column("FacilityId"); 
            Map(m => m.NDR_TX_CURR);
            Map(m => m.NDR_TX_NEW);
            Map(m => m.ReportPeriod);
            Map(m => m.CachedDatetime);
        }
    }

    public class NDR_FacilityMap : ClassMap<NDR_Facilities>
    {
        public NDR_FacilityMap()
        {
            Table("temp_NDR_Facility");

            Id(i => i.Id);
            Map(x => x.IP);
            Map(x => x.State);
            Map(x => x.LGA);
            Map(x => x.LGA_Code);
            Map(x => x.AlternativeLGA);
            Map(x => x.Facility);
            Map(x => x.DATIMCode);
            Map(x => x.GSM);
        }
    }

}
