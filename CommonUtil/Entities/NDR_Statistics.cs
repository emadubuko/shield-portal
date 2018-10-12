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


    public class NDR_Facilities
    {
        public virtual int Id { get; set; }
        public virtual string IP { get; set; }
        public virtual string State { get; set; }
        public virtual string LGA { get; set; }
        public virtual string LGA_Code { get; set; }
        public virtual string AlternativeLGA { get; set; }
        public virtual string Facility { get; set; }
        public virtual string DATIMCode { get; set; }
        public virtual bool GSM { get; set; }
    }

}
