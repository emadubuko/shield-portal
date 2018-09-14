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

}
