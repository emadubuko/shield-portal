using System.Collections.Generic;

namespace DAL.Entities
{
    public class Processes
    {
        public virtual string ImplementingPartnerMEProcess { get; set; }
        public virtual string SiteSupport { get; set; }

        public virtual List<string> ReportLevel { get; set; }

        //public virtual string Audits { get; set; }
        //public virtual string DataCollationAndAnalysis { get; set; }

        public virtual List<DataCollation> DataCollation { get; set; }

        public virtual string DataGarnering { get; set; }
        public virtual string DataUse { get; set; }
        public virtual string DataImprovementApproach { get; set; }
    }

    public class DataCollation
    {
        public int Id { get; set; }
        public string DataType { get; set; }
        public string ReportingLevel { get; set; }
        public string CollationFrequency { get; set; }

    }
}
