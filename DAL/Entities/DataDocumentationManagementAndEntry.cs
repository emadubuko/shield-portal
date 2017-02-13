namespace DAL.Entities
{
    public class DataDocumentationManagementAndEntry
    {
        public virtual int Id { get; set; }
        public virtual string ReportingLevel { get; set; }
        public virtual string ThematicArea { get; set; }
        public virtual string StoredDocumentationAndDataDescriptors { get; set; }
        public virtual string NamingStructureAndFilingStructures { get; set; }
    }
}
