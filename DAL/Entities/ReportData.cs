using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ReportData  
    {
      public virtual string NameOfReport { get; set; }
        public virtual string ThematicArea { get; set; }
        public virtual string FrequencyOfDataCollection { get; set; }
        public virtual string Datatype { get; set; }
        public virtual string Dataformat { get; set; }

        //Data collection and reporting tools (Datasources)
        public virtual string DataCollectionAndReportingTools { get; set; }

        //Data Flow Chart(Diagram)
        public virtual string DataFlowChart { get; set; }
    }
}
