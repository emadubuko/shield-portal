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
    
    public partial class dqa_pivot_table
    {
        public int Id { get; set; }
        public int TB_ART { get; set; }
        public int TX_CURR { get; set; }
        public int PMTCT_ART { get; set; }
        public int OVC { get; set; }
        public bool SelectedForDQA { get; set; }
        public string SelectedReason { get; set; }
        public string Quarter { get; set; }
        public int Year { get; set; }
        public int UploadId { get; set; }
        public long FacilityId { get; set; }
        public int IP { get; set; }
    
        public virtual HealthFacility HealthFacility { get; set; }
        public virtual dqa_pivot_table_upload dqa_pivot_table_upload { get; set; }
        public virtual ImplementingPartner ImplementingPartner { get; set; }
    }
}
