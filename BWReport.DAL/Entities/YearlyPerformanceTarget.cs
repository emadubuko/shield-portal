using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWReport.DAL.Entities
{
    public class YearlyPerformanceTarget
    {
        public virtual int Id { get; set; }
        public virtual DateTime FiscalYearFrom { get; set; }

        public virtual DateTime FiscalYearTo { get; set; }

        public virtual HealthFacility HealthFacilty { get; set; }

        public virtual string HTC_TST { get; set; }

        public virtual string HTC_TST_POS { get; set; }

        public virtual string Tx_NEW { get; set; }
    }
}
