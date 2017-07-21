using BWReport.DAL.Entities;
using FluentNHibernate.Mapping;

namespace BWReport.DAL.Mapping
{
    public class bwrHealthFacilitiesMap : ClassMap<bwrHealthFacility>
    {
        public bwrHealthFacilitiesMap()
        {
            Id(x => x.Id);
            Map(x => x.FacilityName);
            Map(m => m.OrganizationType);
            References(x => x.LGA).Column("LGAId").Not.Nullable();
            Map(x => x.Longitude);
            Map(x => x.Latitude); 
            References(x => x.Organization).Column("ImplementingPartnerId").Not.Nullable(); 
        }
    }
}
