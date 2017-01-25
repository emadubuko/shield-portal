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
            Id(x => x.Id);
            Map(x => x.FiscalYearFrom);
            Map(x => x.FiscalYearTo);
            Map(x => x.HTC_TST);
            Map(x => x.HTC_TST_POS);
            Map(x => x.Tx_NEW);
            References(x => x.HealthFacilty).Column("HealthFaciltyId"); 
        }
    }
}
