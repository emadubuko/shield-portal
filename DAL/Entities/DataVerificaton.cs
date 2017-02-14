using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class DataVerificaton
    {       
        public virtual int Id { get; set; }
        public virtual string DataVerificationApproach { get; set; }
        public virtual string TypesOfDataVerification { get; set; }
         
        public virtual string ReportingLevel { get; set; }
        public virtual string ThematicArea { get; set; }
        public virtual List<DateTime> TimelinesForDataVerification { get; set; }
        public virtual string FrequencyOfDataVerification { get; set; }
        public virtual int DurationOfDataVerificaion { get; set; }

    }
}