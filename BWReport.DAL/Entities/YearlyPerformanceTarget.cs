using CommonUtil.Entities;
using System.Xml.Serialization;

namespace BWReport.DAL.Entities
{
    public class YearlyPerformanceTarget
    {
        [XmlIgnore]
        public virtual int Id { get; set; }
        
        public virtual int FiscalYear { get; set; }
        public virtual string FacilityReportName { get; set; }

        public virtual HealthFacility HealthFacilty { get; set; }

        [XmlIgnore]
        public virtual int HealthFaciltyId { get; set; }

        public virtual int HTC_TST { get; set; }

        public virtual int HTC_TST_POS { get; set; }

        public virtual int Tx_NEW { get; set; }
    }

    public class LGAGroupedYearlyPerformanceTarget
    {
        public string lga_name { get; set; }
        public string lga_code { get; set; }
        public int HTC_Target { get; set; }     
        public int HTC_TST_POS_Target { get; set; }         
        public int Tx_New_Target { get; set; } 
    }
}
