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
    
    public partial class dqa_indicator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dqa_indicator()
        {
            this.dqa_report_value = new HashSet<dqa_report_value>();
        }
    
        public int Id { get; set; }
        public string IndicatorCode { get; set; }
        public string IndicatorName { get; set; }
        public string Readonly { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dqa_report_value> dqa_report_value { get; set; }
    }
}
