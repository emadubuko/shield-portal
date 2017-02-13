namespace DAL.Entities
{
    public class DataAccessAndSharing
    {
        public virtual int Id { get; set; }
        public virtual string ReportingLevel { get; set; }
        public virtual string ThematicArea { get; set; }
        public virtual string DataAccess { get; set; }
        public virtual string DataSharingPolicies { get; set; }
        public virtual string DataTransmissionPolicies { get; set; }
        public virtual string SharingPlatForms { get; set; }
    }
}
