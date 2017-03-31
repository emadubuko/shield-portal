using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Entities
{
   public  class VersionAuthor
    {       
        public virtual string TitleOfAuthor { get; set; }
        public virtual string SurnameAuthor { get; set; }
        public virtual string FirstNameOfAuthor { get; set; }
        public virtual string OtherNamesOfAuthor { get; set; }
        public virtual string JobDesignation { get; set; }
        public virtual string SignatureOfAuthor { get; set; }
        public virtual string PhoneNumberOfAuthor { get; set; }
        public virtual string EmailAddressOfAuthor { get; set; }

        [XmlIgnore]
        public virtual string DisplayName
        {
            get
            {
                return string.Format("{0} {1} {2}", TitleOfAuthor, FirstNameOfAuthor, SurnameAuthor);
            }
        }
    }
}
