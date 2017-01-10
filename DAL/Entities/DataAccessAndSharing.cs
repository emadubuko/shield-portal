using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DataAccessAndSharing
    {
        public virtual string DataAccess { get; set; }
        public virtual string DataSharingPolicies { get; set; }
        public virtual string DataTransmissionPolicies { get; set; }
        public virtual string SharingPlatForms { get; set; }
    }
}
