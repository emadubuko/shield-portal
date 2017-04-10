using CommonUtil.Entities;
using System;
using System.Collections.Generic;

namespace EP.DAL.Entities
{
    public class Evaluation
    {
        public virtual int Id { get; set; }
        public  virtual Organizations ImplementingPartner { get; set; }
        public virtual string ProgramName { get; set; }
        public virtual IList<EvaluationActivities> Activities { get; set; }
        public virtual SupplementaryInfo SupplementaryInfo { get; set; }
        public virtual string ExpectedOutcome { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual DateTime LastUpdatedDate { get; set; }
        public virtual Profile CreatedBy { get; set; }
        public virtual EPStatus Status { get; set; }
    }

    public enum EPStatus
    {
        Yet_to_start, Ongoing, Completed,
    }
}
