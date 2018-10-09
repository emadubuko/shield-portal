using System;

namespace CommonUtil.Entities
{

    public class NDR_Statistics
    {
        public virtual int Id { get; set; }
        public virtual string FacilityCode { get; set; }
        public virtual HealthFacility Facility { get; set; }        
        public virtual int NDR_TX_CURR { get; set; }
        public virtual int NDR_TX_NEW { get; set; }      
        public virtual string ReportPeriod { get; set; }
        public virtual DateTime CachedDatetime { get; set; }
    }

}
