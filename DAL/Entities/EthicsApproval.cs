using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class EthicsApproval
    {
        public virtual string EthicalApprovalForTheProject { get; set; }

        public virtual string Rational { get; set; }
        public virtual string AprrovingInstititionalReviewBoard { get; set; }

        public virtual string TypeOfEthicalApproval { get; set; }
    }
}
