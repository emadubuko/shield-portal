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
    
    public partial class dqa_versions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dqa_versions()
        {
            this.dqa_lnk_version_indicators = new HashSet<dqa_lnk_version_indicators>();
        }
    
        public int id { get; set; }
        public string version_number { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dqa_lnk_version_indicators> dqa_lnk_version_indicators { get; set; }
    }
}