using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DMP
    {
        public virtual int Id { get; set; }
        public virtual List<DMPDocument> DMPDocuments { get; set; }
        public virtual ProjectDetails TheProject { get; set; }
        public virtual Organizations Organization { get; set; }
        public virtual string DMPTitle { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        public virtual DateTime DateCreated { get; set; }
        public virtual Profile CreatedBy { get; set; }

    }
}
