using CommonUtil.Entities;

namespace BWReport.DAL.Entities
{
    public class bwrHealthFacility
    {       
        public virtual int Id { get; set; } 
        public virtual string FacilityName { get; set; }
        public virtual LGA LGA { get; set; } 
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }
        public virtual Organizations Organization { get; set; }  
        public virtual CommonUtil.Enums.OrganizationType OrganizationType { get; set; }
    } 
}
