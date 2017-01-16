using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DataCollection
    {
        public virtual string FrequencyOfDataCollection { get; set; }
        public virtual string DataType { get; set; }
        public virtual string DataCollectionAndReportingTools { get; set; }

        public virtual string DataSources { get; set; }

        public virtual string DataCollectionTimelines { get; set; }

        public virtual string DurationOfDataCollection { get; set; }        
    }
}
