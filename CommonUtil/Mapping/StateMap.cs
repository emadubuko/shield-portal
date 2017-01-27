using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class StateMap : ClassMap<State>
    {
        public StateMap()
        {
            Table("states");
            Id(x => x.state_code);
            Map(x => x.state_name); 
            Map(x => x.geo_polictical_region);
        }
    }
}
