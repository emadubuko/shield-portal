using System.Xml.Serialization;

namespace DAL.Entities
{
    public class Environment
    {
        public virtual string StatesCoveredByImplementingPartners { get; set; }
               
        public virtual AreaCoveredByIP NumberOfSitesCoveredByImplementingPartners { get; set; }        

        public virtual string NoOfSitesCoveredByIP
        {
            get
            {
                if(NumberOfSitesCoveredByImplementingPartners == null)
                {
                    return "";
                }
                else
                {
                  return  string.Format("ART: {0}\n PMTCT: {1}\n HTC: {2}\n OVC: {3}\n Community: {4}",
                        NumberOfSitesCoveredByImplementingPartners.ART, NumberOfSitesCoveredByImplementingPartners.PMTCT,
                        NumberOfSitesCoveredByImplementingPartners.HTC, NumberOfSitesCoveredByImplementingPartners.OVC,
                        NumberOfSitesCoveredByImplementingPartners.Commmunity);                     
                }
            }
        }
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
