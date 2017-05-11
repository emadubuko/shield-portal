using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQA.DAL.Model
{
   public class dqaQ1Fy17Analysis
    {
        public string IP { get; set; }
        public string Facility { get; set; }
        public double? DQA_FY17Q1_HTC_TST { get; set; }
        public double? Validated_HTC_TST { get; set; }
        public string CR_HTC_TST { get; set; }
        public double? DQA_FY17Q1_PMTCT_STAT { get; set; }
        public double? Validated_PMTCT_STAT { get; set; }
        public string CR_PMTCT_STAT { get; set; }
        public double? DQA_FY17Q1_PMTCT_EID { get; set; }
        public double? Validated_PMTCT_EID { get; set; }
        public string CR_PMTCT_EID { get; set; }
        public double? DQA_FY17Q1_PMTCT_ARV { get; set; }
        public double? Validated_PMTCT_ARV { get; set; }
        public string CR_PMTCT_ARV { get; set; }
        public double? DQA_FY17Q1_TX_NEW { get; set; }
        public double? Validated_TX_NEW { get; set; }
        public string CR_TX_NEW { get; set; }
        public double? DQA_FY17Q1_TX_CURR { get; set; }
        public double? Validated_TX_CURR { get; set; }
        public string CR_TX_CURR { get; set; }
        public double? Count_Fails { get; set; }
    }
}
