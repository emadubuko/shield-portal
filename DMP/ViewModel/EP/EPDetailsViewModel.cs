using EP.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel.EP
{
    public class EPDetailsViewModel
    {
        public Evaluation Evaluation { get; set; }
        public IEnumerable<ActivityViewModel> Activities { get; set;}
    }

    public class ActivityViewModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ExpectedOutcome { get; set; }
        public string Status { get; set; }
        public IEnumerable<CommentViewModel> Comments { get; set; }
    }

    public class CommentViewModel
    {
        public int id { get; set; }
        public int activityId { get; set; }
        public string dateadded { get; set; }
        public string commenter { get; set; }
        public string message { get; set; }
    }

}