using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class ProfileMap : ClassMap<Profile>
    {
        public ProfileMap()
        {
            Table("dmp_Profile");
            Id(x => x.Id);
            Map(x => x.FirstName);
            Map(x => x.Surname).Not.Nullable();
            Map(x => x.OtherNames);
            Map(x => x.Username);
            Map(x => x.JobDesignation);
            Map(x => x.ContactEmailAddress);
            Map(x => x.ContactPhoneNumber);
            Map(x => x.CreationDate);
            Map(x => x.LastLoginDate);
            Map(x => x.Status);
            Map(x => x.Title);
            References(x => x.Organization).Column("OrganizationId").Not.LazyLoad().Not.Nullable();
            Map(x => x.RoleName);
        }
    }
}
