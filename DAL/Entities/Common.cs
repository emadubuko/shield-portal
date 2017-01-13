using System;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class Common
    {
        [XmlIgnore]
        public virtual Guid Id { get; set; }
        [XmlIgnore]
        public virtual string Comments { get; set; }
    }
}
