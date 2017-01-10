using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Approval
    {
        public virtual string TitleofApprover { get; set; }
        public virtual string SurnameApprover { get; set; }
        public virtual string FirstnameofApprover { get; set; }
        public virtual string OthernamesofApprover { get; set; }
        public virtual string JobdesignationApprover { get; set; }
        public virtual string SignatureofApprover { get; set; }
        public virtual string PhonenumberofApprover { get; set; }
        public virtual string EmailaddressofApprover { get; set; }


    }
}
