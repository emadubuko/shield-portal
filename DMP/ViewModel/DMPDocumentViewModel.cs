using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class DMPDocumentViewModel
    {       
        public DMPViewModel DmpDetails { get; set; }
       public IList<DMPDocumentDetails> Documents { get; set; }
    }
       

    public class DMPDocumentDetails
    {
        public string DocumentTitle { get; set; }
        public string DocumentId { get; set; }
        public int DMPId { get; set; }
        public int PageNumber { get; set; }
        public string DocumentCreator { get; set; }
        public string Status { get; set; }
        public int ReferralCount { get; set; }
        public string LastModifiedDate { get; set; }
        public string CreationDate { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedDate { get; set; }
        public string Version { get; set; }
    }
}