using System;
using System.Collections.Generic;

namespace CommonUtil.Entities
{
    public class RadetUploadReport
    {
        public virtual int Id { get; set; } 
        public virtual Organizations IP { get; set; }
        public virtual Profile UploadedBy { get; set; }
        public virtual int dqa_year { get; set; }
        public virtual string dqa_quarter { get; set; }
        public virtual IList<RadetTable> Uploads { get; set; }
        public virtual DateTime DateUploaded { get; set; }
        public virtual int CurrentYearTx_New { get; set; }
        public virtual string Facility { get; set; }
    }
}
