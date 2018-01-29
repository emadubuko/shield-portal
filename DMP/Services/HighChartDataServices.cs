using DQA.DAL.Business;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ShieldPortal.Services
{
    public class HighChartDataServices
    {

        public AnalyticPageData GetHTCConcurrency(string reportType, string ip = "")
        {
            var data = Utility.GetQ3Analysis(ip, reportType.ToLower().Contains("partners"));
            List<TempFacilityData> tempData = new List<TempFacilityData>();

            foreach (DataRow dr in data.Rows)
            {
                //htc
                var htc = ComputeConcurrence(dr[2], dr[3]);
                List<object> htc_siteData = new List<object> { dr[1].ToString(), htc.concurrence };

                //pmtct_stat
                var pmtct_stat = ComputeConcurrence(dr[5], dr[6]);
                List<object> pmtct_stat_siteData = new List<object> { dr[1].ToString(), pmtct_stat.concurrence };

                //pmtct_art
                var pmtct_art = ComputeConcurrence(dr[8], dr[9]);
                List<object> pmtct_art_siteData = new List<object> { dr[1].ToString(), pmtct_art.concurrence };

                //pmtct_eid
                var pmtct_eid = ComputeConcurrence(dr[11], dr[12]);
                List<object> pmtct_eid_siteData = new List<object> { dr[1].ToString(), pmtct_eid.concurrence };

                //tx_new
                var tx_new = ComputeConcurrence(dr[14], dr[15]);
                List<object> tx_new_siteData = new List<object> { dr[1].ToString(), tx_new.concurrence };

                //tx_curr
                var tx_curr = ComputeConcurrence(dr[17], dr[18]);
                List<object> tx_curr_siteData = new List<object> { dr[1].ToString(), tx_curr.concurrence };

                var temp = new TempFacilityData();
                temp.IP = dr[0].ToString();

                if (htc.datim != 0 && htc.validated != 0)
                {
                    temp.DATIM_HTC_TST = htc.datim;
                    temp.Validated_HTC_TST = htc.validated;
                    temp.htc_Data = htc_siteData;
                }

                if (pmtct_stat.datim != 0 && pmtct_stat.validated != 0)
                {
                    temp.DATIM_PMTCT_STAT = pmtct_stat.datim;
                    temp.Validated_PMTCT_STAT = pmtct_stat.validated;
                    temp.pmtct_stat_Data = pmtct_stat_siteData;
                }

                if (pmtct_art.datim != 0 && pmtct_art.validated != 0)
                {
                    temp.DATIM_PMTCT_ART = pmtct_art.datim;
                    temp.Validated_PMTCT_ART = pmtct_art.validated;
                    temp.pmtct_art_Data = pmtct_art_siteData;
                }

                if (pmtct_eid.datim != 0 && pmtct_eid.validated != 0)
                {
                    temp.DATIM_PMTCT_EID = pmtct_eid.datim;
                    temp.Validated_PMTCT_EID = pmtct_eid.validated;
                    temp.pmtct_eid_Data = pmtct_eid_siteData;
                }

                if (tx_new.datim != 0 && tx_new.validated != 0)
                {
                    temp.DATIM_TX_NEW = tx_new.datim;
                    temp.Validated_TX_NEW = tx_new.validated;
                    temp.tx_new_Data = tx_new_siteData;
                }

                if (tx_curr.datim != 0 && tx_curr.validated != 0)
                {
                    temp.DATIM_TX_CURR = tx_curr.datim;
                    temp.Validated_TX_CURR = tx_curr.validated;
                    temp.tx_curr_Data = tx_curr_siteData;
                }

                //};
                tempData.Add(temp);
            }

            //List<TempFacilityData> QIData = new List<TempFacilityData>();
            //QIData.AddRange(tempData.Where(x => x.htc_Data != null 
            //                                && (Convert.ToInt32(x.htc_Data[1]) < 95
            //                                || Convert.ToInt32(x.htc_Data[1]) > 105)));
            //QIData.AddRange(tempData.Where(x => x.pmtct_stat_Data != null
            //                                && (Convert.ToInt32(x.pmtct_stat_Data[1]) < 95
            //                                || Convert.ToInt32(x.pmtct_stat_Data[1]) > 105)));
            //QIData.AddRange(tempData.Where(x => x.pmtct_art_Data != null
            //                                && (Convert.ToInt32(x.pmtct_art_Data[1]) < 95
            //                                || Convert.ToInt32(x.pmtct_art_Data[1]) > 105)));
            //QIData.AddRange(tempData.Where(x => x.pmtct_eid_Data != null
            //                                && (Convert.ToInt32(x.pmtct_eid_Data[1]) < 95
            //                                || Convert.ToInt32(x.pmtct_eid_Data[1]) > 105)));
            //QIData.AddRange(tempData.Where(x => x.tx_new_Data != null
            //                                && (Convert.ToInt32(x.tx_new_Data[1]) < 95
            //                                || Convert.ToInt32(x.tx_new_Data[1]) > 105)));
            //QIData.AddRange(tempData.Where(x => x.tx_curr_Data != null
            //                                && (Convert.ToInt32(x.tx_curr_Data[1]) < 95
            //                                || Convert.ToInt32(x.tx_curr_Data[1]) > 105)));


            var allDatamodel = PackageData(tempData);

            // var QIDataModel = PackageData(QIData);
            return new AnalyticPageData
            {
                AllDataModel = allDatamodel,
                // QIDataModel = QIDataModel
            };
            //return allDatamodel;
        }

        public AnalyticPageData GetQ4Concurrency(string reportType, string ip = "")
        {
            var data = Utility.GetQ4Analysis(ip, reportType.ToLower().Contains("partners"));
            List<TempFacilityData> tempData = new List<TempFacilityData>();

            foreach (DataRow dr in data.Rows)
            {
                //htc
                var htc = ComputeConcurrence(dr[2], dr[3]);
                List<object> htc_siteData = new List<object> { dr[1].ToString(), htc.concurrence };

                //pmtct_stat
                var pmtct_stat = ComputeConcurrence(dr[5], dr[6]);
                List<object> pmtct_stat_siteData = new List<object> { dr[1].ToString(), pmtct_stat.concurrence };

                //pmtct_art
                var pmtct_art = ComputeConcurrence(dr[8], dr[9]);
                List<object> pmtct_art_siteData = new List<object> { dr[1].ToString(), pmtct_art.concurrence };

                //pmtct_eid
                var pmtct_eid = ComputeConcurrence(dr[11], dr[12]);
                List<object> pmtct_eid_siteData = new List<object> { dr[1].ToString(), pmtct_eid.concurrence };

                //tx_new
                var tx_new = ComputeConcurrence(dr[14], dr[15]);
                List<object> tx_new_siteData = new List<object> { dr[1].ToString(), tx_new.concurrence };

                //tx_curr
                var tx_curr = ComputeConcurrence(dr[17], dr[18]);
                List<object> tx_curr_siteData = new List<object> { dr[1].ToString(), tx_curr.concurrence };

                //tb_stat
                var tb_stat = ComputeConcurrence(dr[20], dr[21]);
                List<object> tb_stat_siteData = new List<object> { dr[1].ToString(), tb_stat.concurrence };

                //tb_art
                var tb_art = ComputeConcurrence(dr[23], dr[24]);
                List<object> tb_art_siteData = new List<object> { dr[1].ToString(), tb_art.concurrence };

                //tx_tb
                var tx_tb = ComputeConcurrence(dr[26], dr[27]);
                List<object> tx_tb_siteData = new List<object> { dr[1].ToString(), tx_tb.concurrence };

                //pmtct_fo
                var pmtct_fo = ComputeConcurrence(dr[29], dr[30]);
                List<object> pmtct_fo_siteData = new List<object> { dr[1].ToString(), pmtct_fo.concurrence };

                //tx_ret
                var tx_ret = ComputeConcurrence(dr[32], dr[33]);
                List<object> tx_ret_siteData = new List<object> { dr[1].ToString(), tx_ret.concurrence };

                //tx_pvls
                var tx_pvls = ComputeConcurrence(dr[35], dr[36]);
                List<object> tx_pvls_siteData = new List<object> { dr[1].ToString(), tx_pvls.concurrence };

                var temp = new TempFacilityData();
                temp.IP = dr[0].ToString();

                if (htc.datim != 0 && htc.validated != 0)
                {
                    temp.DATIM_HTC_TST = htc.datim;
                    temp.Validated_HTC_TST = htc.validated;
                    temp.htc_Data = htc_siteData;
                }

                if (pmtct_stat.datim != 0 && pmtct_stat.validated != 0)
                {
                    temp.DATIM_PMTCT_STAT = pmtct_stat.datim;
                    temp.Validated_PMTCT_STAT = pmtct_stat.validated;
                    temp.pmtct_stat_Data = pmtct_stat_siteData;
                }

                if (pmtct_art.datim != 0 && pmtct_art.validated != 0)
                {
                    temp.DATIM_PMTCT_ART = pmtct_art.datim;
                    temp.Validated_PMTCT_ART = pmtct_art.validated;
                    temp.pmtct_art_Data = pmtct_art_siteData;
                }

                if (pmtct_eid.datim != 0 && pmtct_eid.validated != 0)
                {
                    temp.DATIM_PMTCT_EID = pmtct_eid.datim;
                    temp.Validated_PMTCT_EID = pmtct_eid.validated;
                    temp.pmtct_eid_Data = pmtct_eid_siteData;
                }

                if (tx_new.datim != 0 && tx_new.validated != 0)
                {
                    temp.DATIM_TX_NEW = tx_new.datim;
                    temp.Validated_TX_NEW = tx_new.validated;
                    temp.tx_new_Data = tx_new_siteData;
                }

                if (tx_curr.datim != 0 && tx_curr.validated != 0)
                {
                    temp.DATIM_TX_CURR = tx_curr.datim;
                    temp.Validated_TX_CURR = tx_curr.validated;
                    temp.tx_curr_Data = tx_curr_siteData;
                }

                if (tb_stat.datim != 0 && tb_stat.validated != 0)
                {
                    temp.DATIM_TB_STAT = tb_stat.datim;
                    temp.Validated_TB_STAT = tb_stat.validated;
                    temp.tb_stat_Data = tb_stat_siteData;
                }

                if (tb_art.datim != 0 && tb_art.validated != 0)
                {
                    temp.DATIM_TB_ART = tb_art.datim;
                    temp.Validated_TB_ART = tb_art.validated;
                    temp.tb_art_Data = tb_art_siteData;
                }

                if (tx_tb.datim != 0 && tx_tb.validated != 0)
                {
                    temp.DATIM_TX_TB = tx_tb.datim;
                    temp.Validated_TX_TB = tx_tb.validated;
                    temp.tx_tb_Data = tx_tb_siteData;
                }

                if (pmtct_fo.datim != 0 && pmtct_fo.validated != 0)
                {
                    temp.DATIM_PMTCT_FO = pmtct_fo.datim;
                    temp.Validated_PMTCT_FO = pmtct_fo.validated;
                    temp.pmtct_fo_Data = pmtct_fo_siteData;
                }

                if (tx_ret.datim != 0 && tx_ret.validated != 0)
                {
                    temp.DATIM_TX_RET = tx_ret.datim;
                    temp.Validated_TX_RET = tx_ret.validated;
                    temp.tx_ret_Data = tx_ret_siteData;
                }

                if (tx_pvls.datim != 0 && tx_pvls.validated != 0)
                {
                    temp.DATIM_TX_PVLS = tx_pvls.datim;
                    temp.Validated_TX_PVLS = tx_pvls.validated;
                    temp.tx_pvls_Data = tx_pvls_siteData;
                }
                //};
                tempData.Add(temp);
            }

            var allDatamodel = PackageData(tempData);

            return new AnalyticPageData
            {
                AllDataModel = allDatamodel,
            };
        }

        public AnalyticPageData GetQ1HTCConcurrency(string reportType, string ip = "")
        {
            var data = Utility.GetQ1FY18Analysis(ip, reportType.ToLower().Contains("partners"));
            List<TempFacilityData> tempData = new List<TempFacilityData>();

            foreach (DataRow dr in data.Rows)
            {
                //htc
                var htc = ComputeConcurrence(dr[2], dr[3]);
                List<object> htc_siteData = new List<object> { dr[1].ToString(), htc.concurrence };

                //pmtct_stat
                var pmtct_stat = ComputeConcurrence(dr[5], dr[6]);
                List<object> pmtct_stat_siteData = new List<object> { dr[1].ToString(), pmtct_stat.concurrence };

                //pmtct_art
                var pmtct_art = ComputeConcurrence(dr[8], dr[9]);
                List<object> pmtct_art_siteData = new List<object> { dr[1].ToString(), pmtct_art.concurrence };

                //pmtct_eid
                var pmtct_eid = ComputeConcurrence(dr[11], dr[12]);
                List<object> pmtct_eid_siteData = new List<object> { dr[1].ToString(), pmtct_eid.concurrence };

                //tx_new
                var tx_new = ComputeConcurrence(dr[14], dr[15]);
                List<object> tx_new_siteData = new List<object> { dr[1].ToString(), tx_new.concurrence };

                //tx_curr
                var tx_curr = ComputeConcurrence(dr[17], dr[18]);
                List<object> tx_curr_siteData = new List<object> { dr[1].ToString(), tx_curr.concurrence };

                //tb_stat
                var tb_stat = ComputeConcurrence(dr[20], dr[21]);
                List<object> tb_stat_siteData = new List<object> { dr[1].ToString(), tb_stat.concurrence };

                //tb_art
                var tb_art = ComputeConcurrence(dr[23], dr[24]);
                List<object> tb_art_siteData = new List<object> { dr[1].ToString(), tb_art.concurrence };

                //tx_tb
                var tx_tb = ComputeConcurrence(dr[26], dr[27]);
                List<object> tx_tb_siteData = new List<object> { dr[1].ToString(), tx_tb.concurrence };

                //pmtct_fo
                var pmtct_fo = ComputeConcurrence(dr[29], dr[30]);
                List<object> pmtct_fo_siteData = new List<object> { dr[1].ToString(), pmtct_fo.concurrence };

                //tx_ret
                var tx_ret = ComputeConcurrence(dr[32], dr[33]);
                List<object> tx_ret_siteData = new List<object> { dr[1].ToString(), tx_ret.concurrence };

                //tx_pvls
                var tx_pvls = ComputeConcurrence(dr[35], dr[36]);
                List<object> tx_pvls_siteData = new List<object> { dr[1].ToString(), tx_pvls.concurrence };

                var temp = new TempFacilityData();
                temp.IP = dr[0].ToString();

                if (htc.datim != 0 && htc.validated != 0)
                {
                    temp.DATIM_HTC_TST = htc.datim;
                    temp.Validated_HTC_TST = htc.validated;
                    temp.htc_Data = htc_siteData;
                }

                if (pmtct_stat.datim != 0 && pmtct_stat.validated != 0)
                {
                    temp.DATIM_PMTCT_STAT = pmtct_stat.datim;
                    temp.Validated_PMTCT_STAT = pmtct_stat.validated;
                    temp.pmtct_stat_Data = pmtct_stat_siteData;
                }

                if (pmtct_art.datim != 0 && pmtct_art.validated != 0)
                {
                    temp.DATIM_PMTCT_ART = pmtct_art.datim;
                    temp.Validated_PMTCT_ART = pmtct_art.validated;
                    temp.pmtct_art_Data = pmtct_art_siteData;
                }

                if (pmtct_eid.datim != 0 && pmtct_eid.validated != 0)
                {
                    temp.DATIM_PMTCT_EID = pmtct_eid.datim;
                    temp.Validated_PMTCT_EID = pmtct_eid.validated;
                    temp.pmtct_eid_Data = pmtct_eid_siteData;
                }

                if (tx_new.datim != 0 && tx_new.validated != 0)
                {
                    temp.DATIM_TX_NEW = tx_new.datim;
                    temp.Validated_TX_NEW = tx_new.validated;
                    temp.tx_new_Data = tx_new_siteData;
                }

                if (tx_curr.datim != 0 && tx_curr.validated != 0)
                {
                    temp.DATIM_TX_CURR = tx_curr.datim;
                    temp.Validated_TX_CURR = tx_curr.validated;
                    temp.tx_curr_Data = tx_curr_siteData;
                }

                if (tb_stat.datim != 0 && tb_stat.validated != 0)
                {
                    temp.DATIM_TB_STAT = tb_stat.datim;
                    temp.Validated_TB_STAT = tb_stat.validated;
                    temp.tb_stat_Data = tb_stat_siteData;
                }

                if (tb_art.datim != 0 && tb_art.validated != 0)
                {
                    temp.DATIM_TB_ART = tb_art.datim;
                    temp.Validated_TB_ART = tb_art.validated;
                    temp.tb_art_Data = tb_art_siteData;
                }

                if (tx_tb.datim != 0 && tx_tb.validated != 0)
                {
                    temp.DATIM_TX_TB = tx_tb.datim;
                    temp.Validated_TX_TB = tx_tb.validated;
                    temp.tx_tb_Data = tx_tb_siteData;
                }

                if (pmtct_fo.datim != 0 && pmtct_fo.validated != 0)
                {
                    temp.DATIM_PMTCT_FO = pmtct_fo.datim;
                    temp.Validated_PMTCT_FO = pmtct_fo.validated;
                    temp.pmtct_fo_Data = pmtct_fo_siteData;
                }

                if (tx_ret.datim != 0 && tx_ret.validated != 0)
                {
                    temp.DATIM_TX_RET = tx_ret.datim;
                    temp.Validated_TX_RET = tx_ret.validated;
                    temp.tx_ret_Data = tx_ret_siteData;
                }

                if (tx_pvls.datim != 0 && tx_pvls.validated != 0)
                {
                    temp.DATIM_TX_PVLS = tx_pvls.datim;
                    temp.Validated_TX_PVLS = tx_pvls.validated;
                    temp.tx_pvls_Data = tx_pvls_siteData;
                }
                //};
                tempData.Add(temp);
            }

            var allDatamodel = PackageData(tempData);

            return new AnalyticPageData
            {
                AllDataModel = allDatamodel,
            };
        }
        private HighchartDrilldownModel PackageData(List<TempFacilityData> tempData)
        {
            var hct_data = new List<Datum>();
            List<ChildSeriesData> HTC_ChildData = new List<ChildSeriesData>();

            var pmtct_stat_data = new List<Datum>();
            List<ChildSeriesData> pmtct_stat_ChildData = new List<ChildSeriesData>();

            var pmtct_art_data = new List<Datum>();
            List<ChildSeriesData> pmtct_art_ChildData = new List<ChildSeriesData>();

            var pmtct_eid_data = new List<Datum>();
            List<ChildSeriesData> pmtct_eid_ChildData = new List<ChildSeriesData>();

            var tx_new_data = new List<Datum>();
            List<ChildSeriesData> tx_new_ChildData = new List<ChildSeriesData>();

            var tx_curr_data = new List<Datum>();
            List<ChildSeriesData> tx_curr_ChildData = new List<ChildSeriesData>();

            var tb_stat_data = new List<Datum>();
            List<ChildSeriesData> tb_stat_ChildData = new List<ChildSeriesData>();

            var tb_art_data = new List<Datum>();
            List<ChildSeriesData> tb_art_ChildData = new List<ChildSeriesData>();

            var tx_tb_data = new List<Datum>();
            List<ChildSeriesData> tx_tb_ChildData = new List<ChildSeriesData>();

            var pmtct_fo_data = new List<Datum>();
            List<ChildSeriesData> pmtct_fo_ChildData = new List<ChildSeriesData>();

            var tx_ret_data = new List<Datum>();
            List<ChildSeriesData> tx_ret_ChildData = new List<ChildSeriesData>();

            var tx_pvls_data = new List<Datum>();
            List<ChildSeriesData> tx_pvls_ChildData = new List<ChildSeriesData>();

            List<ConcurrenceRateByPartner> ConcurrenceByPartner = new List<ConcurrenceRateByPartner>();

            foreach (var item in tempData.GroupBy(x => x.IP))
            {

                var htc = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_HTC_TST), item.ToList().Sum(x => x.Validated_HTC_TST));
                hct_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = htc.concurrence
                });
                if (item.ToList().Select(x => x.htc_Data).Count() > 0)
                {
                    HTC_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.htc_Data != null).Select(x => x.htc_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }


                //pmtct_stat
                var pmtct_stat = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_PMTCT_STAT), item.ToList().Sum(x => x.Validated_PMTCT_STAT));
                pmtct_stat_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = pmtct_stat.concurrence
                });
                if (item.ToList().Select(x => x.pmtct_stat_Data).Count() > 0)
                {
                    pmtct_stat_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.pmtct_stat_Data != null).Select(x => x.pmtct_stat_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }
                

                //pmtct_art
                var pmtct_art = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_PMTCT_ART), item.ToList().Sum(x => x.Validated_PMTCT_ART));
                pmtct_art_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = pmtct_art.concurrence
                });
                if (item.ToList().Select(x => x.pmtct_art_Data).Count() > 0)
                {
                    pmtct_art_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.pmtct_art_Data != null).Select(x => x.pmtct_art_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                
                //pmtct_eid
                var pmtct_eid = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_PMTCT_EID), item.ToList().Sum(x => x.Validated_PMTCT_EID));
                pmtct_eid_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = pmtct_eid.concurrence
                });
                if (item.ToList().Select(x => x.pmtct_eid_Data).Count() > 0)
                {
                    pmtct_eid_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.pmtct_eid_Data != null).Select(x => x.pmtct_eid_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }


                //tx_new
                var tx_new = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TX_NEW), item.ToList().Sum(x => x.Validated_TX_NEW));
                tx_new_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tx_new.concurrence
                });
                if (item.ToList().Select(x => x.tx_new_Data).Count() > 0)
                {
                    tx_new_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tx_new_Data != null).Select(x => x.tx_new_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }


                //tx_curr
                var tx_curr = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TX_CURR), item.ToList().Sum(x => x.Validated_TX_CURR));
                tx_curr_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tx_curr.concurrence
                });
                if (item.ToList().Select(x => x.tx_curr_Data).Count() > 0)
                {
                    tx_curr_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tx_curr_Data != null).Select(x => x.tx_curr_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                //tb_stat
                var tb_stat = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TB_STAT), item.ToList().Sum(x => x.Validated_TB_STAT));
                tb_stat_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tb_stat.concurrence
                });
                if (item.ToList().Select(x => x.tb_stat_Data).Count() > 0)
                {
                    tb_stat_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tb_stat_Data != null).Select(x => x.tb_stat_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                //tb_art
                var tb_art = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TB_ART), item.ToList().Sum(x => x.Validated_TB_ART));
                tb_art_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tb_art.concurrence
                });
                if (item.ToList().Select(x => x.tb_art_Data).Count() > 0)
                {
                    tb_art_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tb_art_Data != null).Select(x => x.tb_art_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                //tx_tb
                var tx_tb = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TX_TB), item.ToList().Sum(x => x.Validated_TX_TB));
                tx_tb_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tx_tb.concurrence
                });
                if (item.ToList().Select(x => x.tx_tb_Data).Count() > 0)
                {
                    tx_tb_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tx_tb_Data != null).Select(x => x.tx_tb_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                //pmtct_fo
                var pmtct_fo = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_PMTCT_FO), item.ToList().Sum(x => x.Validated_PMTCT_FO));
                pmtct_fo_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = pmtct_fo.concurrence
                });
                if (item.ToList().Select(x => x.pmtct_fo_Data).Count() > 0)
                {
                    pmtct_fo_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.pmtct_fo_Data != null).Select(x => x.pmtct_fo_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                //tx_ret
                var tx_ret = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TX_RET), item.ToList().Sum(x => x.Validated_TX_RET));
                tx_ret_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tx_ret.concurrence
                });
                if (item.ToList().Select(x => x.tx_ret_Data).Count() > 0)
                {
                    tx_ret_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tx_ret_Data != null).Select(x => x.tx_ret_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }

                //tx_pvls
                var tx_pvls = ComputeConcurrence(item.ToList().Sum(x => x.DATIM_TX_PVLS), item.ToList().Sum(x => x.Validated_TX_PVLS));
                tx_pvls_data.Add(new Datum
                {
                    name = item.Key,
                    drilldown = item.Key,
                    y = tx_pvls.concurrence
                });
                if (item.ToList().Select(x => x.tx_pvls_Data).Count() > 0)
                {
                    tx_pvls_ChildData.Add(new ChildSeriesData
                    {
                        id = item.Key,
                        name = item.Key,
                        data = item.ToList().Where(x => x.tx_pvls_Data != null).Select(x => x.tx_pvls_Data).OrderByDescending(x => x[1]).ToList()
                    });
                }


                ConcurrenceByPartner.Add(new ConcurrenceRateByPartner
                {
                    IP = item.Key,
                    HTC_Concurrence = htc.concurrence,
                    PMTCT_STAT_Concurrence = pmtct_stat.concurrence,
                    PMTCT_ART_Concurrence = pmtct_art.concurrence,
                    PMTCT_EID_Concurrence = pmtct_eid.concurrence,
                    TX_NEW_Concurrence = tx_new.concurrence,
                    TX_CURR_Concurrence = tx_curr.concurrence,
                    TB_STAT_Concurrence = tb_stat.concurrence,
                    TB_ART_Concurrence = tb_art.concurrence,
                    TX_TB_Concurrence = tx_tb.concurrence,
                    PMTCT_FO_Concurrence = pmtct_fo.concurrence,
                    TX_RET_Concurrence = tx_ret.concurrence,
                    TX_PVLS_Concurrence = tx_pvls.concurrence

                });
            }

            List<ChildSeriesData> HTC_ChildData_qi = RetrieveQIData(HTC_ChildData);

            var pmtct_stat_ChildData_qi = RetrieveQIData(pmtct_stat_ChildData);
            var pmtct_art_ChildData_qi = RetrieveQIData(pmtct_art_ChildData);
            var pmtct_eid_ChildData_qi = RetrieveQIData(pmtct_eid_ChildData);
            var tx_new_ChildData_qi = RetrieveQIData(tx_new_ChildData);
            var tx_curr_ChildData_qi = RetrieveQIData(tx_curr_ChildData);
            var tb_stat_ChildData_qi = RetrieveQIData(tb_stat_ChildData);
            var tb_art_ChildData_qi = RetrieveQIData(tb_art_ChildData);
            var tx_tb_ChildData_qi = RetrieveQIData(tx_tb_ChildData);
            var pmtct_fo_ChildData_qi = RetrieveQIData(pmtct_fo_ChildData);
            var tx_ret_ChildData_qi = RetrieveQIData(tx_ret_ChildData);
            var tx_pvls_ChildData_qi = RetrieveQIData(tx_pvls_ChildData);

            HighchartDrilldownModel model = new HighchartDrilldownModel
            {
                htc_drilldown = HTC_ChildData,
                htc_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = hct_data } },
                htc_drilldown_QI = HTC_ChildData_qi,

                pmtct_stat_drilldown = pmtct_stat_ChildData,
                pmtct_stat_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = pmtct_stat_data } },
                 pmtct_stat_drilldown_QI = pmtct_stat_ChildData_qi,

                pmtct_art_drilldown = pmtct_art_ChildData,
                pmtct_art_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = pmtct_art_data } },
                pmtct_art_drilldown_QI = pmtct_art_ChildData_qi,

                pmtct_eid_drilldown = pmtct_eid_ChildData,
                pmtct_eid_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = pmtct_eid_data } },
                pmtct_eid_drilldown_QI = pmtct_eid_ChildData_qi,

                tx_new_drilldown = tx_new_ChildData,
                tx_new_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tx_new_data } },
                tx_new_drilldown_QI = tx_new_ChildData_qi,

                tx_curr_drilldown = tx_curr_ChildData,
                tx_curr_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tx_curr_data } },
                tx_curr_drilldown_QI = tx_curr_ChildData_qi,

                tb_stat_drilldown = tb_stat_ChildData,
                tb_stat_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tb_stat_data } },
                tb_stat_drilldown_QI = tb_stat_ChildData_qi,

                tb_art_drilldown = tb_art_ChildData,
                tb_art_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tb_art_data } },
                tb_art_drilldown_QI = tb_art_ChildData_qi,

                tx_tb_drilldown = tx_tb_ChildData,
                tx_tb_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tx_tb_data } },
                tx_tb_drilldown_QI = tx_tb_ChildData_qi,

                pmtct_fo_drilldown = pmtct_fo_ChildData,
                pmtct_fo_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = pmtct_fo_data } },
                pmtct_fo_drilldown_QI = pmtct_fo_ChildData_qi,

                tx_ret_drilldown = tx_ret_ChildData,
                tx_ret_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tx_ret_data } },
                tx_ret_drilldown_QI = tx_ret_ChildData_qi,

                tx_pvls_drilldown = tx_pvls_ChildData,
                tx_pvls_series = new List<ParentSeries> { new ParentSeries { name = "Implementing Partner", colorByPoint = true, data = tx_pvls_data } },
                tx_pvls_drilldown_QI = tx_pvls_ChildData_qi,

                ConcurrenceByPartner = ConcurrenceByPartner
            };
            return model;
        }

        public List<ChildSeriesData> RetrieveQIData(List<ChildSeriesData> data)
        {
            List<ChildSeriesData> QI_Data = new List<ChildSeriesData>();
            foreach (var i in data)
            {
                List<List<object>> _new = new List<List<object>>();
                i.data.ForEach(y =>
                {
                    if (Convert.ToInt32(y[1]) < 95 || Convert.ToInt32(y[1]) > 105)
                    {
                        _new.Add(y);
                    }
                });
                QI_Data.Add(
                    new ChildSeriesData
                    {
                        data = _new,
                        id = i.id,
                        name = i.name
                    });
            }
            return QI_Data;
        }

        public ConcurrenceData ComputeConcurrence(object datimobj, object validatedobj)
        {
            double conc = 0;
            int dtm = 0, validatn = 0;
            int.TryParse(datimobj.ToString(), out dtm);
            int.TryParse(validatedobj.ToString(), out validatn);

            if (dtm == 0 && validatn == 0)
            {
                conc = 100;
            }
            else if (validatn == 0)
            {
                conc = 0;
            }
            else if (dtm == 0)
            {
                conc = 0;
            }
            else
            {
                conc = 100 * (validatn * 1.0 / dtm);
            }
            var data = new ConcurrenceData { datim = dtm, validated = validatn, concurrence = conc };
            return data;
        }


    }
}