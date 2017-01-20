using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class Trainings
    {
        [XmlIgnore]
        public virtual string DataHandlingAndEntry { get; set; }

        public virtual int Id { get; set; }
        public virtual string NameOfTraining { get; set; }
        public virtual List<DateTime> TimelinesForTrainings { get; set; }
        public virtual string FequencyOfTrainings { get; set; }
        public virtual int DurationOfTrainings { get; set; }

    }
}
