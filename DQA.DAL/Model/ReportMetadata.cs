using DQA.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DQA.DAL.Model
{
    public class ReportMetadata
    {
        public ReportMetadata()
        {

        }

        public ReportMetadata(dqa_report_metadata metadata)
        {
            Id = metadata.Id;
            SiteId = metadata.SiteId;
            LgaId = metadata.LgaId;
            StateId = metadata.StateId;
            LgaLevel = metadata.LgaLevel;
            FundingAgency = metadata.FundingAgency;
            ImplementingPartner = metadata.ImplementingPartner;
            FiscalYear = metadata.FiscalYear;
            AssessmentWeek = metadata.AssessmentWeek;
            CreateDate = metadata.CreateDate;
            CreatedBy = metadata.CreatedBy;
            Month = metadata.Month;

        }

        public int Id { get; set; }
        public int SiteId { get; set; }
        public int LgaId { get; set; }
        public int StateId { get; set; }
        public int LgaLevel { get; set; }
        public int FundingAgency { get; set; }
        public int ImplementingPartner { get; set; }
        public string FiscalYear { get; set; }
        public int AssessmentWeek { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string Month { get; set; }
    }
}