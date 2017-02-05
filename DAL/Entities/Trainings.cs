using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Trainings
    {
        public virtual string NameOfTraining { get; set; }

        public virtual string SiteStartDate { get; set; }
        public virtual string SiteEndDate { get; set; }

        public virtual string RegionStartDate { get; set; }
        public virtual string RegionEndDate { get; set; }

        public virtual string HQStartDate { get; set; }
        public virtual string HQEndDate { get; set; }

        public virtual string SiteDisplayDate
        {
            get
            {
                return SiteStartDate + " - " + SiteEndDate;
            }
        }

        public virtual string RegionDisplayDate
        {
            get
            {
                return RegionStartDate + " - " + RegionEndDate;
            }
        }

        public virtual string HQDisplayDate
        {
            get
            {
                return HQStartDate + " - " + HQEndDate;
            }
        }

        //public virtual int Id { get; set; }
        //public virtual string NameOfTraining { get; set; }
        //public virtual List<DateTime> TimelinesForTrainings { get; set; }
        //public virtual string FequencyOfTrainings { get; set; }
        //public virtual int DurationOfTrainings { get; set; }

    }
}
