using System.Collections.Generic;

namespace DAL.Entities
{
    public class DataStorage
    {
        public virtual List<DigitalData> Digital { get; set; }
        public virtual List<NonDigitalData> NonDigital { get; set; }
        public virtual List<DataAccessAndSharing> DataAccessAndSharing { get; set; }
        public virtual List<DataDocumentationManagementAndEntry> DataDocumentationManagementAndEntry { get; set; }
    }
}
