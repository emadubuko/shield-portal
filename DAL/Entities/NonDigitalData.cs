using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class NonDigitalData  
    {
        public  virtual string NonDigitalDataTypes { get; set; }
        public virtual string StorageLocation { get; set; }
        public virtual string SafeguardsAndRequirements { get; set; }
    }
}
