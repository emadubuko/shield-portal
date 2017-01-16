using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class Organizations
    {
        [XmlIgnore]
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string Address { get; set; }
        public virtual string MissionPartner { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual byte[] Logo { get; set; } 
        public virtual string WebSite { get; set; }
        public virtual string Fax { get; set; }

        public virtual OrganizationType OrganizationType { get; set; }

    }

    public enum OrganizationType
    {
        ImplemetingPartner, HealthFacilty, CommunityBasedOrganization,StandAloneLab, 
        StateMinistryOfHealth, FederalMinistryOfHealth
    }
}
