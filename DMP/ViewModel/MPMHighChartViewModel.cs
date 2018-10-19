﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class BaseModel
    {
        public string IP { get; set; }
        public string State { get; set; }
        public string LGA { get; set; }
        public string Facility { get; set; }
        public string AgeGroup { get; set; }
        public string Sex { get; set; }
        public string ReportingPeriod { get; set; }
    }

    public class HTSIndexViewDataModel : BaseModel
    {        
        public string Age_Grouping { get; set; }
        public int POS { get; set; }
        public int NEG { get; set; }
        public decimal Yield { get; set; }
      
        public string TestingType { get; set; }
    }

    public class HTSOtherPITCModel : BaseModel
    {
        public string SDP { get; set; }
        public int POS { get; set; }
        public int NEG { get; set; }
    }

    public class TX_RET : BaseModel
    {
        public string PopulationGroup { get; set; }
        public string IndicatorType { get; set; }
        public int Denominator { get; set; }
        public int Numerator { get; set; }
    }

    public class LinkageViewModel : BaseModel
    {
        public string PopulationGroup { get; set; }
        public int POS { get; set; }
        public int Tx_NEW { get; set; }
        public int Row_Number { get; set; }
    }


    public class PMTCT_VL_ViewModel : BaseModel
    {
        public string Category { get; set; }
        public int Result { get; set; }
        public string ResultGroup { get; set; }
        public string PopulationGroup { get; set; }
    }

    public class PMTCT_Cascade_ViewModel : BaseModel
    {
        public int NewClient { get; set; }
        public int NewlyTested { get; set; }        
        public int NewHIVPos { get; set; }
        public int NewOnART { get; set; }
        public int KnownHIVPos { get; set; }
        public int AlreadyOnART { get; set; }
        public int KnownStatus { get; set; }
        public int EID_Sample_Collected { get; set; }
        public int EID_POS { get; set; }
        public int EID_ART_Initiation { get; set; }
    }

    public class TB_STAT_ViewModel : BaseModel
    {
        public int TB_Screened { get; set; }
        public int TB_Presumptive { get; set; }
        public int TB_Presumptive_Diagnosed { get; set; }
        public int TB_Bacteriology_Diagnosis { get; set; }
        public int TB_Diagnosed { get; set; }
        
    }

    public class TB_Treatment_ViewModel : BaseModel
    {
        public int New_Cases { get; set; }
        public int TX_TB { get; set; } 
    }

    public class TB_TPT_ViewModel : BaseModel
    {
        public int Started_on_TPT { get; set; }
        public int PLHIV_eligible_for_TPT { get; set; }
    }
}