using System.Collections.Generic;

namespace DAL.Entities
{
    public class Report
    { 
        public virtual List<ReportData> ReportData { get; set; }
        
    }
}
