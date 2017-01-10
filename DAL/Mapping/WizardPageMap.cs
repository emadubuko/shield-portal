using DAL.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Mapping
{
    //public class WizardPageMap : ClassMap<WizardPage>
    //{
    //    public WizardPageMap()
    //    {
    //        Id(x => x.Id);
    //        References(x => x.DataAccessAndSharing).Column("DataAccessAndSharingId");
    //        References(x => x.DataCollection).Column("DataCollectionId");
    //        References(x => x.DataCollectionProcesses).Column("DataCollectionProcessesId");
    //        References(x => x.DataDocumentationManagementAndEntry).Column("DataDocumentationManagementAndEntryId");
    //        References(x => x.DataStorage).Column("DataStorageId");
    //        References(x => x.DocumentRevisions).Column("DocumentRevisionsId");
    //        References(x => x.IntellectualPropertyCopyrightAndOwnership).Column("IntellectualPropertyCopyrightAndOwnershipId");
    //        References(x => x.Planning).Column("PlanningId");
    //        References(x => x.PostProjectDataRetentionSharingAndDestruction).Column("PostProjectDataRetentionSharingAndDestructionId");
    //        References(x => x.ProjectProfile).Column("ProjectProfileId");
    //        References(x => x.QualityAssurance).Column("QualityAssuranceId");
    //        Map(x => x.Comments);
    //    }
    //}
}
