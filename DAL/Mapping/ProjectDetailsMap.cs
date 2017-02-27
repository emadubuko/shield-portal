using DAL.Entities;
using FluentNHibernate.Mapping;

namespace DAL.Mapping
{
    public class ProjectDetailsMap : ClassMap<ProjectDetails>
    {
        public ProjectDetailsMap()
        {
            Table("dmp_projectdetails");
            Id(x => x.Id);
            Map(x => x.ProjectTitle);
            Map(x => x.GrantReferenceNumber);
            Map(x => x.ProjectStartDate);
            Map(x => x.ProjectEndDate);
            Map(x => x.ProjectSummary);
            References(x => x.LeadActivityManager).Column("LeadActivityManagerId");
            References(x => x.Organization).Column("OrganizationId");

        }
    }
}
