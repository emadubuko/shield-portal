using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class StateMap : ClassMap<State>
    {
        public StateMap()
        {
            Id(x => x.ID);
            Map(x => x.Name);
            Map(x => x.StateCode);
        }
    }
}
