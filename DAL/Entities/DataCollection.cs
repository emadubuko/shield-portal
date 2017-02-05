using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DataCollection
    {
        public virtual string QuantitativeDataCollectionProcessess { get; set; }
        //public virtual string DataFormat { get; set; }
        public virtual string QuantitativeDataSourceCollectionAndReportingTools { get; set; }


        public virtual string QualitativeDataCollectionProcessess { get; set; }
        public virtual string QualitativeDataSourceCollectionAndReportingTools { get; set; }

        //public virtual int Id { get; set; }
        //public virtual string FrequencyOfDataCollection { get; set; }
        //public virtual string DataType { get; set; }
        //public virtual string DataCollectionAndReportingTools { get; set; }

        //public virtual string DataSources { get; set; }

        //public virtual List<DateTime> DataCollectionTimelines { get; set; }

        //public virtual int DurationOfDataCollection { get; set; }        
    }
}
