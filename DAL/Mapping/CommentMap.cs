using DAL.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mapping
{
    public class CommentMap : ClassMap<Comment>
    {
        public CommentMap()
        {
            Id(x => x.Id);
            Map(x => x.Message);
            Map(x => x.TagName);
            Map(x => x.DateAdded);
            Map(x => x.Commenter);
            References(x => x.DMPDocument).Column("DocumentId");
        }
    }
}
