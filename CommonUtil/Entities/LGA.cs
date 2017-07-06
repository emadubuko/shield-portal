namespace CommonUtil.Entities
{
    public class LGA
    {
        public virtual string lga_code { get; set; }

        public virtual string lga_name { get; set; }

        public virtual State State { get; set; }

        public virtual string lga_hm_longcode { get; set; }
        public virtual string alternative_name { get; set; }

        public virtual string DisplayName
        {
            get
            {
                if (this == null) return "";
                return string.Format("{0} ({1})", lga_name, State.state_name);
            }
        }
    }
}
 