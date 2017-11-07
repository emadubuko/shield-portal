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
using RADET.DAL.DAO;

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


        /// <summary>
        /// this is a method to download the DQA tool
        /// </summary>
        /// <param name="facilities">< list of facility pivot table reported in DATIM/param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<string> GenerateDQA(List<PivotTableModel> facilities, Organizations ip)
        {
            string fileName = "DQA_Q4_" + ip.ShortName + ".zip";
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q4/DQA_Q4_" + ip.ShortName);

            string zippedFile = directory + "\\" + fileName;

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                try
                {
                    string[] filenames = Directory.GetFiles(directory);
                    foreach (var file in filenames)//incase the folder is not empty
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }

            var artSites = Utilities.GetARTSiteWithDATIMCode();

            //site name come from pivot table and names match the data in health facility in the database
            foreach (var site in facilities)
            {
                string radetSite;
                artSites.TryGetValue(site.FacilityCode, out radetSite);

                if (string.IsNullOrEmpty(radetSite))
                {
                    radetSite = site.FacilityName;
                }

                string template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q4/Q4_Data Quality Assessment tool_v4.xlsm");
                using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
                {
                    var radet = new RadetMetaDataDAO().RetrieveRadetLineListingForDQA("Q4 FY17", site.IP, radetSite);// site.FacilityName);
                    radet = radet.Where(x => !x.MetaData.Supplementary).ToList();
                    if (radet.Count > 107)
                    {
                        radet.Shuffle();
                        radet = radet.Take(107).ToList();
                    }
                    int tx_current_count = radet.Count;

                    int viral_load_count = radet.Count(x => !string.IsNullOrEmpty(x.CurrentViralLoad));
                    int viral_load_count_suppression = 0;


                    if (radet != null)
                    {
                        var sheet = package.Workbook.Worksheets["TX_CURR"];

                        int row = 1;
                        for (int i = 0; i < radet.Count(); i++)
                        {
                            row++;
                            sheet.Cells["A" + row].Value = radet[i].RadetPatient.PatientId;
                            sheet.Cells["B" + row].Value = radet[i].RadetPatient.HospitalNo;
                            sheet.Cells["C" + row].Value = radet[i].RadetPatient.Sex;
                            sheet.Cells["D" + row].Value = radet[i].RadetPatient.Age_at_start_of_ART_in_years == 0 ? "" : radet[i].RadetPatient.Age_at_start_of_ART_in_years.ToString();
                            sheet.Cells["E" + row].Value = radet[i].RadetPatient.Age_at_start_of_ART_in_months == 0 ? "" : radet[i].RadetPatient.Age_at_start_of_ART_in_months.ToString();
                            sheet.Cells["F" + row].Value = radet[i].ARTStartDate;// string.Format("{0:d-MMM-yyyy}", radet[i].ARTStartDate);
                            sheet.Cells["F" + row].Style.Numberformat.Format = "d-MMM-yyyy";
                            sheet.Cells["G" + row].Value = radet[i].LastPickupDate; // string.Format("{0:d-MMM-yyyy}", radet[i].LastPickupDate);
                            sheet.Cells["G" + row].Style.Numberformat.Format = "d-MMM-yyyy";

                            sheet.Cells["J" + row].Value = radet[i].MonthsOfARVRefill;

                            sheet.Cells["M" + row].Value = radet[i].RegimenLineAtARTStart;
                            sheet.Cells["N" + row].Value = radet[i].RegimenAtStartOfART;

                            sheet.Cells["Q" + row].Value = radet[i].CurrentRegimenLine;
                            sheet.Cells["R" + row].Value = radet[i].CurrentARTRegimen;
                            sheet.Cells["U" + row].Value = radet[i].PregnancyStatus;

                            string currentViralLoad = radet[i].CurrentViralLoad;
                            sheet.Cells["V" + row].Value = currentViralLoad;
                            sheet.Cells["W" + row].Value = radet[i].DateOfCurrentViralLoad; // string.Format("{0:d-MMM-yyyy}", radet[i].DateOfCurrentViralLoad);
                            sheet.Cells["W" + row].Style.Numberformat.Format = "d-MMM-yyyy";
                            sheet.Cells["X" + row].Value = radet[i].ViralLoadIndication;
                            sheet.Cells["Y" + row].Value = radet[i].CurrentARTStatus;

                            if (!string.IsNullOrEmpty(currentViralLoad) && !string.IsNullOrEmpty(currentViralLoad.Trim()))
                            {
                                int cvl = 0;
                                int.TryParse(currentViralLoad, out cvl);
                                if (cvl <= 1000)
                                {
                                    viral_load_count_suppression += 1;
                                }
                            }                            
                        }

                        radet = new RadetMetaDataDAO().RetrieveRadetLineListingForRetebtion("Q4 FY17", new DateTime(2015, 10, 1), new DateTime(2016, 9, 30), site.IP, radetSite);
                        if (radet != null)
                        {
                            List<RADET.DAL.Entities.RadetPatientLineListing> radet_ret = new List<RADET.DAL.Entities.RadetPatientLineListing>();

                            string no_to_select = ExcelHelper.GetRandomizeChartNUmber(radet.Count.ToString());
                            radet.Shuffle();

                            foreach (var item in radet.Take(Convert.ToInt32(no_to_select)))
                            {
                                radet_ret.Add(item);
                            }

                            sheet = package.Workbook.Worksheets["TX_RET"];

                            row = 1;
                            for (int i = 0; i < radet_ret.Count(); i++)
                            {
                                row++;
                                sheet.Cells["A" + row].Value = radet_ret[i].RadetPatient.PatientId;
                                sheet.Cells["B" + row].Value = radet_ret[i].RadetPatient.HospitalNo;
                                sheet.Cells["C" + row].Value = radet_ret[i].ARTStartDate;
                                sheet.Cells["C" + row].Style.Numberformat.Format = "d-MMM-yyyy";
                            }
                        }
                    }

                    var sheetn = package.Workbook.Worksheets["Worksheet"];
                    sheetn.Cells["P2"].Value = site.IP;
                    sheetn.Cells["R2"].Value = site.State;
                    sheetn.Cells["T2"].Value = site.Lga;
                    sheetn.Cells["V2"].Value = site.FacilityName;
                    sheetn.Cells["AA2"].Value = site.FacilityCode;
                    sheetn.Cells["Y2"].Value = "Q4 FY17";

                    sheetn.Cells["N10"].Value = tx_current_count;
                    sheetn.Cells["X10"].Value = viral_load_count;
                    sheetn.Cells["X11"].Value = viral_load_count_suppression;

                    if(viral_load_count > 0)
                    {
                        sheetn.Cells["X12"].Value = 1.0 * viral_load_count_suppression / viral_load_count;
                    }
                    


                    var sheet__all_Q = package.Workbook.Worksheets["All Questions"];
                    //hts_tst
                    sheet__all_Q.Cells["F2"].Value = site.HTS_TST;
                    sheet__all_Q.Cells["F3"].Value = site.HTC_Only_POS + site.PMTCT_STAT_NEW;
                    sheet__all_Q.Cells["F4"].Value = site.HTC_Only;
                    sheet__all_Q.Cells["F5"].Value = site.HTC_Only_POS;
                    //pmtct_stat
                    sheet__all_Q.Cells["F12"].Value = site.PMTCT_STAT;
                    sheet__all_Q.Cells["F13"].Value = site.PMTCT_STAT_NEW;
                    sheet__all_Q.Cells["F14"].Value = site.PMTCT_STAT_PREV;
                    //pmtct_art
                    sheet__all_Q.Cells["F23"].Value = site.PMTCT_ART;
                    //pmtct_eid
                    sheet__all_Q.Cells["F28"].Value = site.PMTCT_EID;
                    //tx_new
                    sheet__all_Q.Cells["F33"].Value = site.TX_NEW;
                    //pmtct_fo
                    sheet__all_Q.Cells["F60"].Value = site.PMTCT_FO;
                    //tb_art
                    sheet__all_Q.Cells["F45"].Value = site.TB_ART;
                    //tb_stat
                    sheet__all_Q.Cells["F39"].Value = site.TB_STAT;
                    //tx_tb
                    sheet__all_Q.Cells["F50"].Value = site.TX_TB;

                    var sheet__all_Summary = package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"];
                    sheet__all_Summary.Cells["E12"].Value = tx_current_count;

                    package.SaveAs(new FileInfo(directory + "/" + site.FacilityName + ".xlsm"));
                }
            }
            await new Utilities().ZipFolder(directory);
            return fileName;
        }
    }
}
