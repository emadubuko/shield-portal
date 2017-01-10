using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class IntellectualPropertyCopyrightAndOwnership 
    {
        public virtual string ContractsAndAgreements { get; set; }
        public virtual string Ownership { get; set; }
        public virtual string UseOfThirdPartyDataSources {get;set;}
    }
}
