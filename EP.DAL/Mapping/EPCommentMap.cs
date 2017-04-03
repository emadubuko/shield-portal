using EP.DAL.Entities;
using FluentNHibernate.Mapping;

namespace EP.DAL.Mapping
{
    public  class EPCommentMap : ClassMap<EPComment>
    {
        public EPCommentMap()
        {
            Table("ep_comment");

            Id(i => i.Id);
            Map(m => m.DateAdded);
            Map(m => m.Message);
            Map(m => m.ResultTagId);
            References(m => m.Commenter);

        }
    }
}
