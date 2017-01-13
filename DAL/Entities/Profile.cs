using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Entities
{
    public class Profile : Common
    {
        public virtual string Title { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public virtual string Surname { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string OtherNames { get; set; }

        public virtual string JobDesignation { get; set; }
        public virtual string ContactPhoneNumber { get; set; }
        public virtual string ContactEmailAddress { get; set; }
        public virtual string FullName
        {
            get
            {
                return string.Format("{0} {1} {2}", Surname, OtherNames, FirstName);
            }
        }

        [XmlIgnore]
        public virtual DateTime CreationDate { get; set; }
        [XmlIgnore]
        public virtual DateTime LastLoginDate { get; set; }

        [XmlIgnore]
        public virtual ProfileStatus Status { get; set; }

        [XmlIgnore]
        public virtual IList<DMPDocument> DMPDocuments { get; set; }
    }

    public enum ProfileStatus
    {
        Disabled, Enabled
    }
}
