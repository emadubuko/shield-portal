using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class HealthFacilityMap : ClassMap<HealthFacility>
    {
        public HealthFacilityMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.FacilityCode).Unique().Not.Nullable();
            References(x => x.LGA).Column("LGAId").Not.Nullable(); 
            Map(x => x.Longitude);
            Map(x => x.Latitude);
            Map(x => x.OrganizationType);
            References(x => x.Organization).Column("ImplementingPartnerId").Not.Nullable();
            References(x => x.Previous_IP).Column("Previous_IP");
            Map(x => x.LinkCode);
            Map(x => x.GranularSite);
            Map(x => x.GSM_2);
        }
    }
}
