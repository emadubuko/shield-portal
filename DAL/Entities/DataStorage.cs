using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DataStorage
    {
        public virtual DigitalData Digital { get; set; }
        public virtual NonDigitalData NonDigital { get; set; }
    }
}
