using DAL.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mapping
{
    public class OrganizationsMap : ClassMap<Organizations>
    {
        public OrganizationsMap()
        {
            Table("dmp_Organizations");
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.ShortName);
            Map(x => x.Address);
            Map(x => x.OrganizationType);
        }
    }
}
