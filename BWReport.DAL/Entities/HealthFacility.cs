using CommonUtil.Entities;
using System.Collections.Generic;

namespace BWReport.DAL.Entities
{
    public class HealthFacility
    {
        public virtual long Id { get; set; }
        public virtual string FacilityCode { get; set; }
        public virtual string Name { get; set; }
        public virtual LGA LGA { get; set; }
        public virtual State State { get; set; }
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }
        public virtual IList<YearlyPerformanceTarget> YearlyPerformanceTarget { get; set; } 
        public virtual IList<PerformanceData> PerfomanceData { get; set; }
    }
}
