using CommonUtil.Entities;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class DMP
    {
        [XmlIgnore]
        public virtual int Id { get; set; }
        [XmlIgnore]
        public virtual List<DMPDocument> DMPDocuments { get; set; }
        [XmlIgnore]
        public virtual ProjectDetails TheProject { get; set; }
        public virtual Organizations Organization { get; set; }
        public virtual string DMPTitle { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        [XmlIgnore]
        public virtual DateTime DateCreated { get; set; }
        [XmlIgnore]
        public virtual Profile CreatedBy { get; set; }

        [XmlIgnore]
        public virtual int OrganizationId { get; set; }

    }
}
