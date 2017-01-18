using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class MonitoringAndEvaluationSystems
    {
        //Data Flow Chart(Diagram)
        public virtual string DataFlowChart { get; set; }

        public virtual RolesAndResponsiblities RoleAndResponsibilities { get; set; }

        public virtual List<Trainings> Trainings { get; set; }
    }
}
