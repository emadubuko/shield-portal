using System;

namespace DAL.Entities
{
    public class MonitoringAndEvaluationSystems
    {
        public virtual People People { get; set; }
        public virtual Processes Process { get; set; }
        public virtual Equipment Equipment { get; set; }
        public virtual Environment Environment { get; set; }
        public virtual DataOrganization Organization { get; set; }
    }
}
