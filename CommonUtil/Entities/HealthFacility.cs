using CommonUtil.Enums;
using System.Xml.Serialization;

namespace CommonUtil.Entities
{
    public class HealthFacility
    {
        [XmlIgnore]
        public virtual int Id { get; set; }
        public virtual string FacilityCode { get; set; }
        [XmlIgnore]
        public virtual string LinkCode { get; set; }
        public virtual string Name { get; set; }
        public virtual LGA LGA { get; set; }

        [XmlIgnore]
        public virtual string lgacode { get; set; }
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }
        public virtual Organizations Organization { get; set; }

        public virtual Organizations Previous_IP { get; set; }
        public virtual OrganizationType OrganizationType { get; set; }
         
    }
}
