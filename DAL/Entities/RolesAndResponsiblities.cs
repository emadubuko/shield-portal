using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class RolesAndResponsiblities
    {
        public virtual string HealthFacilityLevel { get; set; }
        public virtual string AggregationLevel { get; set; }
        public virtual string CentralNationalLevel { get; set; }
    }
}
