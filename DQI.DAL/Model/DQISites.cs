using CommonUtil.Entities;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace DQI.DAL.Model
{
    public class DQIModel
    {
        public List<DQISites> DQISites { get; set; }
        public List<IPLevelDQI> IPLevel { get; set; }
    }

    public class DQISites
    {
        public int Id { get; set; }
        public HealthFacility Facilty { get; set; }
        public string IP { get; set; }
        public string Indicator { get; set; }
        public double DATIM { get; set; }
        public double Validated { get; set; }
        public string indicatorConcurrence { get; set; }
        public string IndicatorDeviation { get; set; } 
    }

    public class IPLevelDQI
    {
        public string IP { get; set; }
        public int NoOfSitesAffected { get; set; } 
        public string AverageConcurrence { get; set; }
        public string WorstPerformingIndicator { get; set; }
        public int TotalSites { get; set; }

        //for dqi dashboard
        public System.DateTime  LastUpdatedDate { get; set; }
        public string UploadedBy { get; set; }
        public int Id { get; set; }
        public string AffectedSites { get; set; }
    }

    //this is data elements for process indicators xml
        
    public class ProcessTable
    {
        public string Name { get; set; }
        public string Numerator_Definition { get; set; }
        public string Denominator_Definition { get; set; }

        [XmlElement(ElementName = "Week_1")]
        public WeekData Week_1 { get; set; }
        [XmlElement(ElementName = "Week_2")]
        public WeekData Week_2 { get; set; }
        [XmlElement(ElementName = "Week_3")]
        public WeekData Week_3 { get; set; }
        [XmlElement(ElementName = "Week_4")]
        public WeekData Week_4 { get; set; }
        [XmlElement(ElementName = "Week_5")]
        public WeekData Week_5 { get; set; }
        [XmlElement(ElementName = "Week_6")]
        public WeekData Week_6 { get; set; }
        [XmlElement(ElementName = "Week_7")]
        public WeekData Week_7 { get; set; }
        [XmlElement(ElementName = "Week_8")]
        public WeekData Week_8 { get; set; }
        [XmlElement(ElementName = "Week_9")]
        public WeekData Week_9 { get; set; }
        [XmlElement(ElementName = "Week_10")]
        public WeekData Week_10 { get; set; }
        [XmlElement(ElementName = "Week_11")]
        public WeekData Week_11 { get; set; }

        [XmlElement(ElementName = "Week_12")]
        public WeekData Week_12 { get; set; }
    }

    public class WeekData
    {
        public int? Numerator { get; set; }
        public int? Denominator { get; set; }
    }
}
