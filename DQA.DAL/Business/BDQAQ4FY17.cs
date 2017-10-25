using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DQA.DAL.Model;

namespace DQA.DAL.Business
{ 
    public class BDQAQ4FY17
    {
        shield_dmpEntities entity;

        public BDQAQ4FY17()
        {
            entity = new shield_dmpEntities();
        }


        public bool ReadPivotTable(Stream datimFile, string reportPeriod, Profile profile, out string result)
        {
            var previously = entity.dqa_pivot_table_upload.FirstOrDefault(x => x.Quarter == reportPeriod.Trim() && x.ImplementingPartner.Id == profile.Organization.Id);
            if (previously != null)
            {
                result = "Pivot table already uploaded"; 
                return false;
                //throw new ApplicationException();
            }

            var hfs = entity.HealthFacilities.ToDictionary(x => x.FacilityCode);
            StringBuilder sb = new StringBuilder();

            List<dqa_pivot_table> pivotTable = new List<dqa_pivot_table>();
            try
            {
                using (ExcelPackage package = new ExcelPackage(datimFile))
                {
                    var worksheet = package.Workbook.Worksheets["Facility Pivot Table"];
                    if (worksheet == null)
                    {
                        throw new ApplicationException("Invalid pivot table uploaded");
                    }

                    int row = 2;

                    while (true)
                    {
                        Data.HealthFacility hf = null;
                        string fCode = ExcelHelper.ReadCell(worksheet, row, 1);
                        string fName = ExcelHelper.ReadCellText(worksheet, row, 2);
                        if (string.IsNullOrEmpty(fCode))
                        {
                            break;
                        }
                        if (hfs.TryGetValue(fCode, out hf) == false)
                        {
                            sb.AppendLine("unknown facility with code [" + fCode + "," + fName + "] uploaded ");
                        }
                        if (hf != null)
                        {
                            if (hf.ImplementingPartner.Id != profile.Organization.Id)
                            {
                                sb.AppendLine("Facility [" + hf.Name + "] does not belong to the your IP [" + profile.Organization.ShortName + "]. Please correct and try again");
                            }

                            //if(hf.Name != fName)
                            //{
                            //    //sb.AppendLine(string.Format("Update dbo.HealthFacility set Name = '{0}' where id= {1}; ", fName, hf.Id));
                            //}

                            int hts_tst = ExcelHelper.ReadCellText(worksheet, row, 3).ToInt();
                            int htc_only = ExcelHelper.ReadCellText(worksheet, row, 4).ToInt();
                            int htc_only_pos = ExcelHelper.ReadCellText(worksheet, row, 5).ToInt();
                            int pmtct_stat = ExcelHelper.ReadCellText(worksheet, row, 6).ToInt();
                            int pmtct_stat_new = ExcelHelper.ReadCellText(worksheet, row, 7).ToInt();
                            int pmtct_stat_prev = ExcelHelper.ReadCellText(worksheet, row, 8).ToInt();
                            int pmtct_art = ExcelHelper.ReadCellText(worksheet, row, 9).ToInt();
                            int pmtct_eid = ExcelHelper.ReadCellText(worksheet, row, 10).ToInt();
                            int pmtct_fo = ExcelHelper.ReadCellText(worksheet, row, 11).ToInt();
                            int tx_new = ExcelHelper.ReadCellText(worksheet, row, 12).ToInt();
                            int tx_curr = ExcelHelper.ReadCellText(worksheet, row, 13).ToInt();
                            int tx_ret = ExcelHelper.ReadCellText(worksheet, row, 14).ToInt();
                            int tx_pvls = ExcelHelper.ReadCellText(worksheet, row, 15).ToInt();
                            int tb_stat = ExcelHelper.ReadCellText(worksheet, row, 16).ToInt();
                            int tb_art = ExcelHelper.ReadCellText(worksheet, row, 17).ToInt();
                            int tx_tb = ExcelHelper.ReadCellText(worksheet, row, 18).ToInt();

                            pivotTable.Add(new dqa_pivot_table
                            {
                                HTS_TST = hts_tst,
                                HTC_Only = htc_only,
                                HTC_Only_POS = htc_only_pos,
                                PMTCT_STAT = pmtct_stat,
                                PMTCT_STAT_NEW = pmtct_stat_new,
                                PMTCT_STAT_PREV = pmtct_stat_prev,
                                PMTCT_ART = pmtct_art,
                                PMTCT_EID = pmtct_eid,
                                PMTCT_FO = pmtct_fo,
                                TX_NEW = tx_new,
                                TX_CURR = tx_curr,
                                TX_RET = tx_ret,
                                TX_PVLS = tx_pvls,
                                TB_STAT = tb_stat,
                                TB_ART = tb_art,
                                TX_TB = tx_tb, 
                                HealthFacility = hf,
                                Quarter = reportPeriod, 
                                ImplementingPartner = hf.ImplementingPartner,
                            });
                        }
                        row += 1;
                    }

                    worksheet = package.Workbook.Worksheets["OVC_PIVOT TABLE"];
                    if (worksheet == null)
                    {
                        throw new ApplicationException("Invalid pivot table uploaded");
                    }
                    row = 2;

                    while (true)
                    {
                        Data.HealthFacility hf = null;
                        string fCode = ExcelHelper.ReadCell(worksheet, row, 1);
                        string fName = ExcelHelper.ReadCellText(worksheet, row, 2);
                        if (string.IsNullOrEmpty(fCode))
                        {
                            break;
                        }
                        if (hfs.TryGetValue(fCode, out hf) == false)
                        {
                            sb.AppendLine("unknown facility with code [" + fCode + "," + fName + "] uploaded ");
                        }
                        if (hf != null)
                        {
                            if (hf.ImplementingPartner.Id != profile.Organization.Id)
                            {
                                sb.AppendLine("Facility [" + hf.Name + "] does not belong to the your IP [" + profile.Organization.ShortName + "]. Please correct and try again");
                            }
                            pivotTable.Add(new Data.dqa_pivot_table
                            {
                                 IP = hf.ImplementingPartner.Id,
                                  FacilityId = hf.Id,                                     
                                HealthFacility = hf, 
                                Quarter = reportPeriod,
                                ImplementingPartner = hf.ImplementingPartner,
                                OVC = ExcelHelper.ReadCellText(worksheet, row, 3).ToInt(),
                                OVC_NotReported = ExcelHelper.ReadCellText(worksheet, row, 4).ToInt(),
                            });

                        }
                        row += 1;
                    }
                }

                if (!string.IsNullOrEmpty(sb.ToString()))
                {
                    Logger.LogInfo("pivot table upload", sb.ToString());
                    throw new ApplicationException(sb.ToString());
                }

                List<dqa_pivot_table> selectedList = new List<dqa_pivot_table>();

                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["Use_top_90_for_dqa_site_selection"]))
                {
                    #region top 90%
                    int total_tx = pivotTable.Sum(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART));
                    double _90_percent_of_Total_tx = total_tx * 0.9;
                    pivotTable = pivotTable.OrderByDescending(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART)).ToList();
                    foreach (var item in pivotTable)
                    {
                        selectedList.Add(item);
                        if (selectedList.Sum(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART)) < _90_percent_of_Total_tx)
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "Based on 90% of Tx";
                        }
                        else if (item.OVC > 0)//all ovc sites are selected
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "OVC Site";
                        }
                    }
                    #endregion
                }
                else
                {
                    #region top 80% and randomized 50% of remaining 

                    //calclaute 80% of tx_curr
                    int total_tx_curr = pivotTable.Sum(x => (x.TX_CURR + x.PMTCT_ART));
                    double _80_percent_of_Total_tx_curr = total_tx_curr * 0.8;

                    //ensures the highest contributor are top of the list
                    pivotTable = pivotTable.OrderByDescending(x => (x.TX_CURR + x.PMTCT_ART)).ToList();

                    foreach (var item in pivotTable)
                    {
                        selectedList.Add(item);
                        if (selectedList.Sum(x => (x.TX_CURR + x.PMTCT_ART)) <= _80_percent_of_Total_tx_curr)
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "Based on 80% of Tx_curr";
                        }
                        else if (item.OVC > 0)
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "OVC Site";
                        }
                    }

                    //Randomize and select 50% (half) of the ones not marked above
                    var remaining = selectedList.Where(x => !x.SelectedForDQA).ToList();
                    remaining.Shuffle();
                    for (int i = 0; i < remaining.Count() / 2; i++)
                    {
                        selectedList.FirstOrDefault(x => x == remaining[i]).SelectedForDQA = true;
                        selectedList.FirstOrDefault(x => x == remaining[i]).SelectedReason = "Based on randomization";
                    }
                    #endregion
                }

                ////delete previous submissions
                //var previously = entity.dqa_pivot_table_upload.FirstOrDefault(x => x.Quarter == reportPeriod.Trim() && x.ImplementingPartner.Id == profile.Organization.Id);
                //if (previously != null)
                //{
                //    entity.dqa_pivot_table_upload.Remove(previously);
                //}

                entity.dqa_pivot_table_upload.Add(
                    new dqa_pivot_table_upload
                    {  
                        DateUploaded = DateTime.Now,
                        dqa_pivot_table = selectedList,
                        IP = profile.Organization.Id,
                        Quarter = reportPeriod,
                        UploadedBy = profile.Id
                    });

                entity.dqa_pivot_table.RemoveRange(entity.dqa_pivot_table.Where(x => x.Quarter == reportPeriod.Trim() && x.ImplementingPartner.Id == profile.Organization.Id).ToList());
                entity.dqa_pivot_table.AddRange(selectedList);
                entity.SaveChanges();

                result = Newtonsoft.Json.JsonConvert.SerializeObject(
                    from item in selectedList
                    select new
                    {
                        FacilityName = item.HealthFacility.Name,
                        item.HTS_TST,
                        item.HTC_Only,
                        item.HTC_Only_POS,
                        item.PMTCT_STAT,
                        item.PMTCT_STAT_NEW,
                        item.PMTCT_STAT_PREV,
                        OVC = item.OVC + item.OVC_NotReported,
                        item.PMTCT_ART,
                        item.PMTCT_EID,
                        item.PMTCT_FO,
                        item.TX_NEW,                        
                        item.TX_CURR,
                        item.TX_RET,
                        item.TX_PVLS,
                        item.TB_STAT,
                        item.TB_ART,
                         item.TX_TB,
                        item.SelectedForDQA,
                        item.SelectedReason
                    }
                 );

                // new CommonUtil.DAO.HealthFacilityDAO().RunSQL(sb.ToString());

            }
            catch (Exception ex)
            {
                result = ex.Message;
                Logger.LogError(ex);
                return false;
            }
            return true;
        }

        public void DeletePivotTable(int id)
        {
            var data = entity.dqa_pivot_table_upload.First(x => x.Id == id);
            entity.dqa_pivot_table.RemoveRange(data.dqa_pivot_table).ToList();
            entity.dqa_pivot_table_upload.Remove(data);
            entity.SaveChanges();
        }

        public string ReadWorkbook(string extractedFilePath, Profile userUploading)
        {
            throw new NotImplementedException();
        }

        public string GetReportDetails(int metadataid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateDQA(List<PivotTableModel> data, Organizations organization)
        {
            throw new NotImplementedException();
        }
    }
}
