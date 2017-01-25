using System;

namespace BWReport.DAL.Entities
{
    public class PerformanceData
    {
        public virtual long Id { get; set; }
        public virtual string HTC_TST { get; set; }
        public virtual string HTC_TST_POS { get; set; }
        public virtual string Tx_NEW { get; set; }
        public virtual DateTime ReportPeriodFrom { get; set; }
        public virtual DateTime ReportPeriodTo { get; set; }

        public virtual HealthFacility HealthFacility { get; set; }
        public virtual YearlyPerformanceTarget FiscalYearTarget { get; set; }
        public virtual ReportUploads ReportUpload { get; set; }
    }
}
