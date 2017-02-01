namespace DAL.Entities
{
    public class Environment
    {
        public virtual string StatesCoveredByImplementingPartners { get; set; }
        public virtual AreaCoveredByIP NumberOfSitesCoveredByImplementingPartners { get; set; }        
    }

    public class AreaCoveredByIP
    {
        public virtual int ART { get; set; }
        public virtual int PMTCT { get; set; }
        public virtual int HTC { get; set; }
        public virtual int OVC { get; set; }
        public virtual int Commmunity { get; set; }
    }
}
