using System.Collections.Generic;
using System.Xml.Serialization;

namespace DAL.Entities
{
    [XmlRoot("Root")]
    public class WizardPage
    {
        public virtual ProjectProfile ProjectProfile { get; set; }
        public virtual List<DocumentRevisions> DocumentRevisions { get; set; }
        public virtual Planning Planning { get; set; }
        public virtual MonitoringAndEvaluationSystems MonitoringAndEvaluationSystems { get; set; }
        public virtual DataProcesses DataProcesses { get; set; }
        public virtual QualityAssurance QualityAssurance { get; set; }
        public virtual DataStorage DataStorageAccessAndSharing { get; set; }
        public virtual IntellectualPropertyCopyrightAndOwnership IntellectualPropertyCopyrightAndOwnership { get; set; }
        public virtual PostProjectDataRetentionSharingAndDestruction PostProjectDataRetentionSharingAndDestruction { get; set; }

    }
}
