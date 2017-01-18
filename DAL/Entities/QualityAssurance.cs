using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class QualityAssurance 
    {
        public virtual List<DataVerificaton> DataVerification { get; set; }
    }
}
