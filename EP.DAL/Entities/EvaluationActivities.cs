using System;
using System.Collections.Generic;

namespace EP.DAL.Entities
{
    public class EvaluationActivities
    {
        public virtual int Id { get; set; }
        public virtual Evaluation TheEvaluation { get; set; }
        public virtual string Name { get; set; }
        public virtual string ExpectedOutcome { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual EPStatus Status { get; set; }
        public virtual IList<EPComment> Comments { get; set; }
    }

    
}
