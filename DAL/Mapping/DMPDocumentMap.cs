using DAL.Entities;
using DAL.Utilities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DAL.Mapping
{
    public class DMPDocumentMap : ClassMap<DMPDocument>
    {
        public DMPDocumentMap()
        {
            Table("dmp_XMLDocument");
            Id(x => x.Id);
            Map(x => x.Document).CustomType(typeof(XmlType<WizardPage>));
            Map(x => x.PageNumber);
            References(x => x.Initiator).Column("InitiatorId").Not.Nullable();
            Map(x => x.InitiatorUsername).Not.Nullable();
            Map(x => x.ReferralCount);
            Map(x => x.LastModifiedDate);
            Map(x => x.Status);
            Map(x => x.Version);
            References(x => x.TheDMP).Column("DMPId");
            References(x => x.ApprovedBy).Column("AproverId");
            Map(x => x.ApprovedDate);
            Map(x => x.CreationDate);
        }
    }
}
