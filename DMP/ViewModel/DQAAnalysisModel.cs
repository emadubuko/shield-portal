using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class DQAAnalysisModel
    {
        public string IP { get; set; }
        public string Facility { get; set; }
        public string HTC { get; set; }
        public string Validate_HTC { get; set; }
        public string Concurrence_rate_HTC { get; set; }
        public string PMTCT_STAT { get; set; }
        public string Validate_PMTCT_STAT { get; set; }
        public string Concurrence_rate_PMTCT_STAT { get; set; }
        public string PMTCT_ART { get; set; }
        public string Validate_PMTCT_ART { get; set; }
        public string Concurrence_rate_PMTCT_ART { get; set; }
        public string PMTCT_EID { get; set; }
        public string Validate_PMTCT_EID { get; set; }
        public string Concurrence_rate_PMTCT_EID { get; set; }
        public string TX_NEW { get; set; }
        public string Validate_TX_NEW { get; set; }
        public string Concurrence_rate_TX_NEW { get; set; }
        public string TX_Curr { get; set; }
        public string Validate_TX_Curr { get; set; }
        public string Concurrence_rate_TX_Curr { get; set; }
        public string PMTCT_HEI_POS { get; set; }
        public string Validate_PMTCT_HEI_POS { get; set; }
        public string Concurrence_rate_PMTCT_HEI_POS { get; set; }
        public string TB_STAT { get; set; }
        public string Validate_TB_STAT { get; set; }
        public string Concurrence_rate_TB_STAT { get; set; }
        public string TB_ART { get; set; }
        public string Validate_TB_ART { get; set; }
        public string Concurrence_rate_TB_ART { get; set; }
        public string TX_TB { get; set; }
        public string Validate_TX_TB { get; set; }
        public string Concurrence_rate_TX_TB { get; set; }
        public string PMTCT_FO { get; set; }
        public string Validate_PMTCT_FO { get; set; }
        public string Concurrence_rate_PMTCT_FO { get; set; }
        public string TX_RET { get; set; }
        public string Validate_TX_RET { get; set; }
        public string Concurrence_rate_TX_RET { get; set; }
        public string TX_PLVS { get; set; }
        public string Validate_TX_PLVS { get; set; }
        public string Concurrence_rate_TX_PLVS { get; set; }
        public string ContactEmailAddress { get; set; }
        public string State_name { get; set; }
        public string Lga_name { get; set; }
    }

    public class DQAFailCountComparisonModel
    {
        public string IP { get; set; }
        public string Facility { get; set; }         
        public string State { get; set; }
        public string Lga { get; set; }
        public int? Q2_FAIL_COUNT { get; set; }
        public int? Q1_FAIL_COUNT { get; set; }

    }
}
