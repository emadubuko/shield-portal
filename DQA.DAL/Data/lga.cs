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
    
    public partial class lga
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public lga()
        {
            this.HealthFacilities = new HashSet<HealthFacility>();
            this.HealthFacilities1 = new HashSet<HealthFacility>();
        }
    
        public string lga_code { get; set; }
        public string state_code { get; set; }
        public string lga_name { get; set; }
        public string lga_hm_longcode { get; set; }
        public string alternative_name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HealthFacility> HealthFacilities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HealthFacility> HealthFacilities1 { get; set; }
        public virtual state state { get; set; }
        public virtual state state1 { get; set; }
        public virtual state state2 { get; set; }
        public virtual state state3 { get; set; }
    }
}
