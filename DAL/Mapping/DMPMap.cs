using DAL.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mapping
{
    public class DMPMap : ClassMap<DMP>
    {
        public DMPMap()
        {
            Id(x => x.Id);
            Map(x => x.DMPTitle);
            References(x => x.TheProject).Column("TheProjectId");
            References(x => x.Organization).Column("OrganizationId");
            Map(x => x.StartDate);
            Map(x => x.EndDate);
            References(x => x.CreatedBy);
            Map(x => x.DateCreated);
        }
    }
}
