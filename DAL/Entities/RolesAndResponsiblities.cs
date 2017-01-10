using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class RolesAndResponsiblities  
    {
        public virtual string HealthFacility { get; set; }
        public virtual string ImplementingPartner { get; set; }
        public virtual string LGA { get; set; }
        public virtual string StateMoH { get; set; }
        public virtual string CDC { get; set; }
        public virtual string FMoH { get; set; }

    }
}
