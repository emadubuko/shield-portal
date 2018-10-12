using CommonUtil.Entities;
using System.Collections.Generic;

namespace RADET.DAL.Entities
{
    public class RadetMetaData : IEntity
    {
        public virtual int Id { get; set; }
        public virtual string RadetPeriod { get; set; }
        public virtual string Facility { get; set; }
        public virtual string FacilityDatimCode { get; set; }
        public virtual LGA LGA { get; set; }
        public virtual Organizations IP { get; set; }

        public virtual Organizations FY12_17_IP { get; set; }

        public virtual IList<RadetPatientLineListing> PatientLineListing { get; set; }
        public virtual RadetUpload RadetUpload { get; set; }
        public virtual bool Supplementary { get; set; }
    }
}
