using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class PostProjectDataRetentionSharingAndDestruction
    {
        public virtual string DataToRetain { get; set; }
        public virtual string PreExistingData { get; set; }
        public virtual string Duration { get; set; }
        public virtual string Licensing { get; set; }
        public virtual DigitalDataRetention DigitalDataRetention { get; set; }
        public virtual NonDigitalDataRetention NonDigitalRentention { get; set; }
    }
}
