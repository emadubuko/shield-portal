using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class LGAMap : ClassMap<LGA>
    {
        public LGAMap()
        {
            Id(x => x.ID);
            Map(x => x.Name);
            References(x => x.State).Column("StateId");
        }
    }
}
