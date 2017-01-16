using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DataVerificaton
    {
        public virtual string DataVerificationApproach { get; set; }
        public virtual string TypesOfDataVerification { get; set; }

        public virtual string FormsOfDataVerification { get; set; }
        public virtual string TimelinesForDataVerification { get; set; }
        public virtual string FrequencyOfDataVerification { get; set; }
        public virtual string DurationOfDataVerificaion { get; set; }

    }
}