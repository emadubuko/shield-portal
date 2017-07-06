using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class LGAMap : ClassMap<LGA>
    {
        public LGAMap()
        {
            Table("lga");
            Id(x => x.lga_code);
            Map(x => x.lga_name);
            Map(x => x.lga_hm_longcode);
            Map(x => x.alternative_name);
            References(x => x.State).Column("state_code").Not.LazyLoad();
        }
    }
}
