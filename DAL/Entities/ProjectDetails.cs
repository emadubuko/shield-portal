using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class ProjectDetails
    {
        [XmlIgnore]
        public virtual int Id { get; set; }
        public virtual string ProjectTitle { get; set; }
        public virtual string DocumentTitle { get; set; }
        public virtual string NameOfImplementingPartner { get; set; }
        public virtual string AbreviationOfImplementingPartner { get; set; }
        public virtual int OrganizationId { get; set; }
        public virtual string AddressOfOrganization { get; set; }
        public virtual string MissionPartner { get; set; }
        public virtual string ProjectStartDate { get; set; }
        public virtual string ProjectEndDate { get; set; }
        public virtual string GrantReferenceNumber {get;set;}
        public virtual string ProjectSummary { get; set; }

        public virtual Profile LeadActivityManager { get; set; }
        

        [XmlIgnore]
        public virtual Organizations Organization { get; set; }

    }
}
