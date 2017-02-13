namespace DAL.Entities
{
    public class NonDigitalData  
    {
        public virtual int Id { get; set; }
        public virtual string ReportingLevel { get; set; }
        public virtual string ThematicArea { get; set; }
        public  virtual string NonDigitalDataTypes { get; set; }
        public virtual string StorageLocation { get; set; }
        public virtual string SafeguardsAndRequirements { get; set; }
    }
}
