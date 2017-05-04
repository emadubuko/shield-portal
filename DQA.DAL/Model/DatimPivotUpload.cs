using System;
using System.Collections.Generic;

namespace DQA.DAL.Model
{
    public class PivotUpload
    {
        public string FacilityName { get; set; } 
        public int TB_ART { get; set; }
        public int TX_CURR { get; set; }
        public int PMTCT_ART { get; set; }
        public int OVC { get; set; }
        public bool SelectedForDQA { get; set; }
        public string SelectedReason { get; set; }
    }

    public class UploadList
    {
        public string id { get; set; }
        public string IP { get; set; }
        public string Period { get; set; } 
        public DateTime DateUploaded { get; set; }
        public string UploadedBy { get; set; }

        public IEnumerable<PivotUpload> Tables { get; set; }
    }
}
