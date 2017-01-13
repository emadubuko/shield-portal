using DAL.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mapping
{
    public class ProjectDetailsMap : ClassMap<ProjectDetails>
    {
        public ProjectDetailsMap()
        {
            Table("dmp_ProjectDetails");
            Id(x => x.Id);
            Map(x => x.ProjectTitle);
            Map(x => x.GrantReferenceNumber);
            Map(x => x.ProjectStartDate);
            Map(x => x.ProjectEndDate);
            Map(x => x.ProjectSummary);
            References(x => x.Organization).Column("OrganizationId");
        }
    }
}
