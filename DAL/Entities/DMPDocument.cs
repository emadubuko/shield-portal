using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DMPDocument : Common
    {       
        public virtual WizardPage Document { get; set; }
        public virtual DMP TheDMP { get; set; }
        public virtual int PageNumber { get; set; }
        public virtual Profile Initiator { get; set; }
        public virtual string InitiatorUsername { get; set; }
        public virtual DMPStatus Status { get; set; }
        public virtual int ReferralCount { get; set; }
        public virtual DateTime LastModifiedDate { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual Profile ApprovedBy { get; set; }
        public virtual DateTime? ApprovedDate { get; set; }
        public virtual string Version { get; set; }
        
       
        public virtual string DocumentTitle
        {
            get
            {
                if (Document.ProjectProfile != null & Document.ProjectProfile.ProjectDetails != null)
                {
                    return Document.ProjectProfile.ProjectDetails.DocumentTitle;
                }
                return "";
            }
        }

    }

    public enum DMPStatus
    {
        New, PendingApproval, ReferredBack, Rejected, Approved
    }
}
