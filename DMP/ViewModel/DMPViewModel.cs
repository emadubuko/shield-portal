using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class DMPViewModel
    {
        public string Title { get; set; }
        public string  ProjectTitle { get; set; }
        public string Owner { get; set; }
        public string CreatedBy { get; set; } 
        public string Status { get; set; }
        public string DateCreated { get; set; }
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}