using CommonUtil.Entities;
using System.Collections.Generic;

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
    }
}
