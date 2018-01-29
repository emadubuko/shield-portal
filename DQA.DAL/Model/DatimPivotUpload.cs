﻿using System;
using System.Collections.Generic;

namespace DQA.DAL.Model
{
    public class PivotTableModel
    {
        public int Id { get; set; }
        public string FacilityName { get; set; } 
        public int TB_ART { get; set; }
        public int TX_CURR { get; set; }
        public int PMTCT_ART { get; set; }
        public int? HTS_TST { get; set; }
        public int? HTC_Only { get; set; }
        public int? HTC_Only_POS { get; set; }
        public int? PMTCT_STAT { get; set; }
        public int? PMTCT_STAT_NEW { get; set; }
        public int? PMTCT_STAT_PREV { get; set; }
        public int? PMTCT_EID { get; set; }
        public int? TX_NEW { get; set; }

        public int OVC_Total { get; set; }
        public bool SelectedForDQA { get; set; }
        public string SelectedReason { get; set; }
        public string State { get; set; }
        public string Lga { get; set; } 
        public Data.lga TheLGA { get; set; }
        public string IP { get; set; }
        public string FacilityCode { get; set; }
        public int? PMTCT_FO { get; set; }
        public int? TX_RET { get; set; }
        public int? TX_PVLS { get; set; }
        public int? TB_STAT { get; set; }
        public int? TX_TB { get; set; }

        public int? PMTCT_HEI_POS { get; set; }
    }

    public class UploadList
    {
        public string id { get; set; }
        public string IP { get; set; }
        public string Period { get; set; } 
        public DateTime DateUploaded { get; set; }
        public string UploadedBy { get; set; }

        public IEnumerable<PivotTableModel> Tables { get; set; }
    }

    public class SelectedFacilities
    {
        public List<int> selectedFacilities { get; set; }
    }
}
