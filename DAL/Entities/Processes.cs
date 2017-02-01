namespace DAL.Entities
{
    public class Processes
    {
        public virtual string ImplementingPartnerMEProcess { get; set; }
        public virtual string SiteVisits { get; set; }
        public virtual string Audits { get; set; }
        public virtual string DataCollationAndAnalysis { get; set; }
    }
}
