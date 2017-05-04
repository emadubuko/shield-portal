using CommonUtil.Entities;
using CommonUtil.Utilities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public  class AfenetReportMap : ClassMap<AfenetReport>
    {
        public AfenetReportMap()
        {
            Id(i => i.Id);
            Map(m => m.Facility);
            Map(m => m.Services);
            Map(m => m.State);
            Map(m => m.IP);
            Map(m => m.HTC_TST).CustomType(typeof(XmlType<IndicatorScore>));
            Map(m => m.HTC_TST_POS).CustomType(typeof(XmlType<IndicatorScore>));
            Map(m => m.PMTCT_ARV).CustomType(typeof(XmlType<IndicatorScore>));
            Map(m => m.PMTCT_STAT).CustomType(typeof(XmlType<IndicatorScore>));
            Map(m => m.PMTCT_STAT_POS).CustomType(typeof(XmlType<IndicatorScore>));
            Map(m => m.TX_CURR).CustomType(typeof(XmlType<IndicatorScore>));
            Map(m => m.TX_NEW).CustomType(typeof(XmlType<IndicatorScore>)); 
        }
    }
}
