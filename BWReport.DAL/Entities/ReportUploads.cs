using System;

namespace BWReport.DAL.Entities
{
    public class ReportUploads
    {
        public virtual long Id { get; set; } 
        public virtual DateTime DateUploaded { get; set; }
        public virtual string  ImplementingPartner { get; set; } 
        public virtual string UploadingUser { get; set; } 
        public virtual string ReportingPeriod { get; set; }
        public virtual int FY { get; set; }

        public virtual string ReportPeriodDisplayName
        {
            get
            {
                return ReportingPeriod + " " + FY;
            }
        }

        //public virtual DateTime ReportingPeriodTo { get; set; }
        //public virtual DateTime ReportingPeriodFrom { get; set; }       
    }
}
