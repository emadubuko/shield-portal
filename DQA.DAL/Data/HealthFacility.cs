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
    
    public partial class HealthFacility
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FacilityCode { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string LGAId { get; set; }
        public string OrganizationType { get; set; }
        public Nullable<int> ImplementingPartnerId { get; set; }
    
        public virtual lga lga { get; set; }
        public virtual ImplementingPartner ImplementingPartner { get; set; }
        public virtual lga lga1 { get; set; }
        public virtual ImplementingPartner ImplementingPartner1 { get; set; }
    }
}
