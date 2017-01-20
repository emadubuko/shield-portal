using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ReportData  
    {
        public virtual int Id { get; set; }
        public virtual string NameOfReport { get; set; }
        public virtual string ThematicArea { get; set; }
        public virtual List<DateTime> TimelinesForReporting { get; set; }
        public virtual string FrequencyOfReporting { get; set; }
        public virtual int DurationOfReporting { get; set; }          
    }
}
