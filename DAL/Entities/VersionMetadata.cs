using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class VersionMetadata  
    {
        public virtual string VersionDate { get; set; }
        public virtual string VersionNumber { get; set; }
    }
}
