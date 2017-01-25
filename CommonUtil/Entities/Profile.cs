using System;
using System.Xml.Serialization;

namespace CommonUtil.Entities
{
    public class Profile
    {
        [XmlIgnore]
        public virtual Guid Id { get; set; }

        public virtual string Title { get; set; }
        public virtual string Username { get; set; }
        public virtual string FullName
        {
            get
            {
                return string.Format("{0} {1} {2} {3}", Title, Surname, OtherNames, FirstName);
            }
        }
        public virtual string Password { get; set; }
        public virtual string Surname { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string OtherNames { get; set; }

        public virtual string JobDesignation { get; set; }
         
        public virtual string ContactPhoneNumber { get; set; }

       
        public virtual string ContactEmailAddress { get; set; }

        public virtual Organizations Organization { get; set; }

        
        public virtual string RoleName { get; set; }

        [XmlIgnore]
        public virtual int OrganizationId { get; set; }

        [XmlIgnore]
        public virtual DateTime CreationDate { get; set; }
        [XmlIgnore]
        public virtual DateTime LastLoginDate { get; set; }

        [XmlIgnore]
        public virtual ProfileStatus Status { get; set; }
         
    }

    public enum ProfileStatus
    {
        Disabled, Enabled
    }
}
