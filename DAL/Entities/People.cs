using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class People
    {
        public virtual string DataFlowChart { get; set; }
        public virtual string StaffingInformation { get; set; }
        public virtual string Staffing { get; set; }
        public virtual string RoleAndResponsibilities { get; set; }
        public virtual string DataHandlingAndEntry { get; set; }
        public virtual List<Trainings> Trainings { get; set; }
    }
}
