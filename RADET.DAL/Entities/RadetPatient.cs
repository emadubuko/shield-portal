using System.Collections.Generic;

namespace RADET.DAL.Entities
{
    public class RadetPatient
    {
        public virtual int Id { get; set; }
        public virtual string PatientId { get; set; }
        public virtual string HospitalNo { get; set; }
        public virtual string Sex { get; set; }
        public virtual int? Age_at_start_of_ART_in_years { get; set; }
        public virtual int? Age_at_start_of_ART_in_months { get; set; }

        public virtual CommonUtil.Entities.Organizations IP { get; set; }
        public virtual string FacilityName { get; set; }

        public virtual IList<RadetPatientLineListing> PatientLineListing { get; set; }
    }

     
}
