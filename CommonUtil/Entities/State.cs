using System.Collections.Generic;

namespace CommonUtil.Entities
{
    public class State
    {
        public virtual int ID { get; set; }
        public virtual string state_name { get; set; }
        public virtual string state_code { get; set; }
        public virtual string geo_polictical_region { get; set; }

        public virtual List<LGA> LgaList { get; set; }
    }
}
