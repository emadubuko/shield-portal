//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DQA.DAL.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class dqa_report_metadata
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string LgaId { get; set; }
        public string StateId { get; set; }
        public int LgaLevel { get; set; }
        public int FundingAgency { get; set; }
        public int ImplementingPartner { get; set; }
        public string FiscalYear { get; set; }
        public int AssessmentWeek { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string ReportPeriod { get; set; }
        public Nullable<int> ReportYear { get; set; }
    }
}
