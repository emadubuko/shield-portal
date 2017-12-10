using System.Collections.Generic;

namespace ShieldPortal.ViewModel
{
    public class AnalyticPageData
    {
        public HighchartDrilldownModel AllDataModel { get; set; }
       // public HighchartDrilldownModel  QIDataModel { get; set; }
    }

    public class HighchartDrilldownModel
    {
        public List<ParentSeries> htc_series { get; set; }
        public List<ChildSeriesData> htc_drilldown { get; set; }
        public List<ChildSeriesData> htc_drilldown_QI { get; set; }

        public List<ParentSeries> pmtct_stat_series { get; set; }
        public List<ChildSeriesData> pmtct_stat_drilldown { get; set; }
        public List<ChildSeriesData> pmtct_stat_drilldown_QI { get; set; }

        public List<ParentSeries> pmtct_art_series { get; set; }
        public List<ChildSeriesData> pmtct_art_drilldown { get; set; }
        public List<ChildSeriesData> pmtct_art_drilldown_QI { get; set; }

        public List<ParentSeries> pmtct_eid_series { get; set; }
        public List<ChildSeriesData> pmtct_eid_drilldown { get; set; }
        public List<ChildSeriesData> pmtct_eid_drilldown_QI { get; set; }

        public List<ParentSeries> tx_new_series { get; set; }
        public List<ChildSeriesData> tx_new_drilldown { get; set; }
        public List<ChildSeriesData> tx_new_drilldown_QI { get; set; }

        public List<ParentSeries> tx_curr_series { get; set; }
        public List<ChildSeriesData> tx_curr_drilldown { get; set; }
        public List<ChildSeriesData> tx_curr_drilldown_QI { get; set; }

        public List<ParentSeries> tb_stat_series { get; set; }
        public List<ChildSeriesData> tb_stat_drilldown { get; set; }
        public List<ChildSeriesData> tb_stat_drilldown_QI { get; set; }

        public List<ParentSeries> tb_art_series { get; set; }
        public List<ChildSeriesData> tb_art_drilldown { get; set; }
        public List<ChildSeriesData> tb_art_drilldown_QI { get; set; }

        public List<ParentSeries> tx_tb_series { get; set; }
        public List<ChildSeriesData> tx_tb_drilldown { get; set; }
        public List<ChildSeriesData> tx_tb_drilldown_QI { get; set; }

        public List<ParentSeries> pmtct_fo_series { get; set; }
        public List<ChildSeriesData> pmtct_fo_drilldown { get; set; }
        public List<ChildSeriesData> pmtct_fo_drilldown_QI { get; set; }

        public List<ParentSeries> tx_ret_series { get; set; }
        public List<ChildSeriesData> tx_ret_drilldown { get; set; }
        public List<ChildSeriesData> tx_ret_drilldown_QI { get; set; }

        public List<ParentSeries> tx_pvls_series { get; set; }
        public List<ChildSeriesData> tx_pvls_drilldown { get; set; }
        public List<ChildSeriesData> tx_pvls_drilldown_QI { get; set; }

        public List<ConcurrenceRateByPartner> ConcurrenceByPartner { get; set; }    
    }

    public class Datum
    {
        public string name { get; set; }
        public double y { get; set; }
        public string drilldown { get; set; }
    }

    public class ParentSeries
    {
        public string name { get; set; }
        public bool colorByPoint { get; set; }
        public List<Datum> data { get; set; }
    }

    public class ChildSeriesData
    {
        public string name { get; set; }
        public string id { get; set; }
        public List<List<object>> data { get; set; }
    }

    public class TempFacilityData
    {
        public string IP { get; set; }
        public int DATIM_HTC_TST { get; set; }
        public int Validated_HTC_TST { get; set; }
        public List<object> htc_Data { get; set; }

        public int DATIM_PMTCT_STAT { get; set; }
        public int Validated_PMTCT_STAT { get; set; }
        public List<object> pmtct_stat_Data { get; set; }

        public int DATIM_PMTCT_ART { get; set; }
        public int Validated_PMTCT_ART { get; set; }
        public List<object> pmtct_art_Data { get; set; }

        public int DATIM_PMTCT_EID { get; set; }
        public int Validated_PMTCT_EID { get; set; }
        public List<object> pmtct_eid_Data { get; set; }

        public int DATIM_TX_NEW { get; set; }
        public int Validated_TX_NEW { get; set; }
        public List<object> tx_new_Data { get; set; }

        public int DATIM_TX_CURR { get; set; }
        public int Validated_TX_CURR { get; set; }
        public List<object> tx_curr_Data { get; set; }

        public int DATIM_TB_STAT { get; set; }
        public int Validated_TB_STAT { get; set; }
        public List<object> tb_stat_Data { get; set; }

        public int DATIM_TB_ART { get; set; }
        public int Validated_TB_ART { get; set; }
        public List<object> tb_art_Data { get; set; }

        public int DATIM_TX_TB { get; set; }
        public int Validated_TX_TB { get; set; }
        public List<object> tx_tb_Data { get; set; }

        public int DATIM_PMTCT_FO { get; set; }
        public int Validated_PMTCT_FO { get; set; }
        public List<object> pmtct_fo_Data { get; set; }

        public int DATIM_TX_RET { get; set; }
        public int Validated_TX_RET { get; set; }
        public List<object> tx_ret_Data { get; set; }

        public int DATIM_TX_PVLS { get; set; }
        public int Validated_TX_PVLS { get; set; }
        public List<object> tx_pvls_Data { get; set; }
    }
    public class ConcurrenceData
    {
        public double concurrence { get; set; }
        public int datim { get; set; }
        public int validated { get; set; }
    }

    public class ConcurrenceRateByPartner
    {
        public string IP { get; set; }
        public double HTC_Concurrence { get; set; }
        public double PMTCT_STAT_Concurrence { get; set; } 
        public double PMTCT_ART_Concurrence { get; set; }  
        public double PMTCT_EID_Concurrence { get; set; }  
        public double TX_NEW_Concurrence { get; set; }  
        public double TX_CURR_Concurrence { get; set; }
        public double TB_STAT_Concurrence { get; set; }
        public double PMTCT_FO_Concurrence { get; set; }
        public double TB_ART_Concurrence { get; set; }
        public double TX_RET_Concurrence { get; set; }
        public double TX_TB_Concurrence { get; set; }
        public double TX_PVLS_Concurrence { get; set; }
    }
}