using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class OrganizationsMap : ClassMap<Organizations>
    {
        public OrganizationsMap()
        {
            Table("ImplementingPartners");
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.ShortName);
            Map(x => x.Address);
            Map(x => x.MissionPartner);
            Map(x => x.Logo).Length(int.MaxValue); 
            Map(x => x.WebSite);
            Map(x => x.Fax);
            Map(x => x.PhoneNumber);
    }
    }
}
