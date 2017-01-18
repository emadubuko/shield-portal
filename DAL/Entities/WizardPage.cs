using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DAL.Entities
{
    [XmlRoot("Root")]
    public class WizardPage
    {
        public virtual ProjectProfile ProjectProfile { get; set; }
        public virtual List<DocumentRevisions> DocumentRevisions { get; set; }
        public virtual Planning Planning { get; set; }

        public virtual DataCollectionProcesses DataCollectionProcesses { get; set; }

        public virtual List<DataCollection> DataCollection { get; set; }
        public virtual MonitoringAndEvaluationSystems MonitoringAndEvaluationSystems { get; set; }
        public virtual QualityAssurance QualityAssurance { get; set; }

        public virtual Report Reports { get; set; }
       
        public virtual DataStorage DataStorage { get; set; }
        public virtual IntellectualPropertyCopyrightAndOwnership IntellectualPropertyCopyrightAndOwnership { get; set; }
        public virtual DataAccessAndSharing DataAccessAndSharing { get; set; }
        public virtual DataDocumentationManagementAndEntry DataDocumentationManagementAndEntry { get; set; }
        public virtual PostProjectDataRetentionSharingAndDestruction PostProjectDataRetentionSharingAndDestruction { get; set; }

    }
}
