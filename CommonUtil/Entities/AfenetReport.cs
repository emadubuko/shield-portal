namespace CommonUtil.Entities
{
    public  class AfenetReport
    {
        public virtual int Id { get; set; }
        public virtual string State { get; set; }
        public virtual string Facility { get; set; }
        public virtual string IP { get; set; }
        public virtual string Services { get; set; }
        public virtual IndicatorScore HTC_TST { get; set; }
        public virtual IndicatorScore HTC_TST_POS { get; set; }
        public virtual IndicatorScore PMTCT_STAT { get; set; }
        public virtual IndicatorScore PMTCT_STAT_POS { get; set; }
        public virtual IndicatorScore PMTCT_ARV { get; set; }
        public virtual IndicatorScore TX_NEW { get; set; }
        public virtual IndicatorScore TX_CURR { get; set; }

    }

    public class IndicatorScore
    {
        public int SAPR16 { get; set; }
        public int Validated { get; set; }
        public decimal Concurrence { get; set; }
    }
}
