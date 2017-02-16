using DAL.Entities;
using FluentNHibernate.Mapping;
using NHibernate.Mapping.ByCode;

namespace DAL.Mapping
{
    public class DMPMap : ClassMap<DMP>
    {
        public DMPMap()
        {
            Table("dmp");

            Id(x => x.Id);
            Map(x => x.DMPTitle);
            References(x => x.TheProject).Column("TheProjectId").Not.LazyLoad();
            References(x => x.Organization).Column("OrganizationId");
            Map(x => x.StartDate);
            Map(x => x.EndDate);
            References(x => x.CreatedBy).Not.LazyLoad();
            Map(x => x.DateCreated);
            //HasMany(x => x.DMPDocuments).Cascade.All()
            //.Inverse().KeyColumns.Add("DMPId", mapping => mapping.Name("DMPId"));

            HasMany(x => x.DMPDocuments)
                .Cascade.None()
                .Inverse()
                .KeyColumns.Add("DMPId", mapping => mapping.Name("DMPId"));
        }
    }
}
