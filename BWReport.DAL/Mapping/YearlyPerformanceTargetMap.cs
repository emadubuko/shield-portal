using BWReport.DAL.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWReport.DAL.Mapping
{
   public class YearlyPerformanceTargetMap : ClassMap<YearlyPerformanceTarget>
    {
        public YearlyPerformanceTargetMap()
        {
            Table("bwr_YearlyPerformanceTarget");
            Id(x => x.Id); 
            Map(x => x.FiscalYear);
            Map(x => x.HTC_TST);
            Map(x => x.HTC_TST_POS);
            Map(x => x.Tx_NEW);
            References(x => x.HealthFacilty).Column("HealthFaciltyId"); 
        }
    }
}
