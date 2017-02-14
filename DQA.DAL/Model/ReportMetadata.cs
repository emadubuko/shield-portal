using DQA.DAL.Business;
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
            SiteId = Utility.GetFacility(metadata.SiteId).Site_Name;
            LgaId = Utility.GetLgaName(metadata.LgaId);
            StateId =Utility.GetStateName(metadata.StateId);
            //LgaLevel = metadata.LgaLevel;
            //FundingAgency = metadata.FundingAgency;
            //ImplementingPartner = metadata.ImplementingPartner;
            FiscalYear = metadata.FiscalYear;
            AssessmentWeek = metadata.AssessmentWeek;
            CreateDate = metadata.CreateDate;
            CreatedBy = metadata.CreatedBy;
            Month = metadata.ReportPeriod;

        }

        public int Id { get; set; }
        public String SiteId { get; set; }
        public String LgaId { get; set; }
        public string StateId { get; set; }
        //public int LgaLevel { get; set; }
        //public int FundingAgency { get; set; }
       // public int ImplementingPartner { get; set; }
        public string FiscalYear { get; set; }
        public int AssessmentWeek { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string Month { get; set; }
    }
}