using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CommonUtil.Entities
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
 
        public virtual List<string> SubscribedApps { get; set; }

       // public virtual OrganizationType OrganizationType { get; set; }

    }

    
}
