using BWReport.DAL.Entities;
using FluentNHibernate.Mapping;

namespace BWReport.DAL.Mapping
{
    public class HealthFacilityMap : ClassMap<HealthFacility>
    {
        public HealthFacilityMap()
        {
            Id(x => x.Id).GeneratedBy.Native();
            Map(x => x.Name);
            Map(x => x.FacilityCode);
            References(x => x.LGA).Column("LGAId");
            References(x => x.State).Column("StateId");
            Map(x => x.Longitude);
            Map(x => x.Latitude);


        }
    }
}
