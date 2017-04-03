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
        public virtual string ExpectedOutcome { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
    }
}
