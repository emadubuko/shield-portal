using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Version  
    {
        public virtual VersionMetadata VersionMetadata { get; set; }
        public virtual VersionAuthor VersionAuthor { get; set; }
        public virtual Approval Approval { get; set; }
    }
}
