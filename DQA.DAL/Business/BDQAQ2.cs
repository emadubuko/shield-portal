using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using DQA.DAL.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using RADET.DAL.DAO;
using System.Text;

namespace DQA.DAL.Business
{
    public class BDQAQ2
    {
        shield_dmpEntities entity;
        public BDQAQ2()
        {
            entity = new shield_dmpEntities();
        }

        public string ReadWorkbook(string filename, string username, string[] datimNumbersRaw)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filename)))
            { 
                //get the metadata of the report
                    var metadata = new dqa_report_metadata();
                try
                {
                    var worksheet = package.Workbook.Worksheets["Worksheet"];
                   
                   var facility_code = worksheet.Cells["AA2"].Value.ToString(); //facility code
                    var facility = entity.HealthFacilities.FirstOrDefault(e => e.FacilityCode == facility_code);
                    if (facility == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. The facility is incorrect</td></tr>";
                    }

                   
                    metadata.AssessmentWeek = 1;
                    metadata.CreateDate = DateTime.Now;
                    metadata.CreatedBy = username;
                    metadata.FiscalYear = DateTime.Now.Year.ToString();
                    metadata.FundingAgency = 1;
                    metadata.ImplementingPartner = facility.ImplementingPartnerId.Value;
                    metadata.LgaId = facility.LGAId;
                    metadata.LgaLevel = 2;
                    metadata.ReportPeriod = worksheet.Cells["Y2"].Value.ToString();
                    metadata.SiteId = Convert.ToInt32(facility.Id);
                    metadata.StateId = facility.lga.state.state_code; // state.state_code;
                     
                    //check if the report exists
                    var meta_count = entity.dqa_report_metadata.Count(e => e.FiscalYear == metadata.FiscalYear && e.FundingAgency == metadata.FundingAgency && e.ReportPeriod == metadata.ReportPeriod && e.ImplementingPartner == metadata.ImplementingPartner && e.SiteId == metadata.SiteId);
                    if (meta_count > 0)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. Report already exists in the database</td></tr>";
                    }
                    entity.dqa_report_metadata.Add(metadata);
                    entity.SaveChanges();
                     
                    worksheet = package.Workbook.Worksheets["All Questions"];

                    //get all the indicators in the system
                    var indicators = entity.dqa_indicator.Where(x => x.DQAPeriod == metadata.ReportPeriod);

                    for (var i = 7; i < 59; i++)
                    {
                         

                        var istValueMonth = worksheet.Cells[i, 4];
                        var sndValueMonth = worksheet.Cells[i, 5];
                        var trdValueMonth = worksheet.Cells[i, 6];
                        var question = worksheet.Cells[i, 2];

                        //check if there is a value for the indicator
                        if (istValueMonth == null || istValueMonth.Value == null || sndValueMonth == null || sndValueMonth.Value == null || trdValueMonth == null || trdValueMonth.Value == null || question == null || question.Value == null)
                            continue;

                        var indicator_code = worksheet.Cells[i, 2].Value.ToString();
                        var indicator = indicators.FirstOrDefault(e => e.IndicatorCode == indicator_code);
                        if (indicator == null)
                            continue;
                        var report_value = new dqa_report_value();
                        report_value.MetadataId = metadata.Id;
                        report_value.IndicatorId = indicator.Id;
                        report_value.IndicatorValueMonth1 = Utility.GetDecimal(istValueMonth.Value);
                        report_value.IndicatorValueMonth2 = Utility.GetDecimal(sndValueMonth.Value);
                        report_value.IndicatorValueMonth3 = Utility.GetDecimal(trdValueMonth.Value);

                        entity.dqa_report_value.Add(report_value);
                    }
                    entity.SaveChanges();

                    ReadSummary(package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"], metadata.Id);

                  //  SaveDQADimensions(package.Workbook, metadata.Id);

                  //  SaveDQAComparison(package.Workbook, metadata.Id, datimNumbersRaw);

                    return "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td>" + filename + " was processed successfully</td></tr>";
                }
                catch (Exception ex)
                {
                    entity.dqa_report_metadata.Remove(metadata);
                    entity.SaveChanges();
                    Logger.LogError(ex);
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>There are errors " + filename + "</td></tr>";
                }
            }
        }

        /// <summary>
        /// Read the result from the summary sheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="medata_data_id"></param>
        private void ReadSummary(ExcelWorksheet worksheet, int medata_data_id)
        {
            int i = 6;
            var summaries = new XElement("summaries");
            var reported_data = new XElement("reported_data");
            reported_data.Add(new XElement("HTC_TST", worksheet.Cells[i, 2].Value.ToString()));
            reported_data.Add(new XElement("HTC_TST_Pos", worksheet.Cells[i, 3].Value.ToString()));
            reported_data.Add(new XElement("HTC_Only", worksheet.Cells[i, 4].Value.ToString()));
            reported_data.Add(new XElement("HTC_Pos", worksheet.Cells[i, 5].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_STAT", worksheet.Cells[i, 6].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_STAT_Pos", worksheet.Cells[i, 7].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_STAT_Knwpos", worksheet.Cells[i, 8].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_ART", worksheet.Cells[i, 9].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_EID", worksheet.Cells[i, 10].Value.ToString()));
            reported_data.Add(new XElement("TX_NEW", worksheet.Cells[i, 11].Value.ToString()));
            reported_data.Add(new XElement("TB_STAT", worksheet.Cells[i, 12].Value.ToString()));
            reported_data.Add(new XElement("TB_ART", worksheet.Cells[i, 13].Value.ToString()));
            reported_data.Add(new XElement("TX_TB", worksheet.Cells[i, 14].Value.ToString()));
            reported_data.Add(new XElement("TB_PREV", worksheet.Cells[i, 15].Value.ToString()));
            reported_data.Add(new XElement("TX_Curr", worksheet.Cells["E12"].Value.ToString()));
            summaries.Add(reported_data);

            i = 7;
            var validation = new XElement("validation");
            validation.Add(new XElement("HTC_TST", worksheet.Cells[i, 2].Value.ToString()));
            validation.Add(new XElement("HTC_TST_Pos", worksheet.Cells[i, 3].Value.ToString()));
            validation.Add(new XElement("HTC_Only", worksheet.Cells[i, 4].Value.ToString()));
            validation.Add(new XElement("HTC_Pos", worksheet.Cells[i, 5].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT", worksheet.Cells[i, 6].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT_Pos", worksheet.Cells[i, 7].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT_Knwpos", worksheet.Cells[i, 8].Value.ToString()));
            validation.Add(new XElement("PMTCT_ART", worksheet.Cells[i, 9].Value.ToString()));
            validation.Add(new XElement("PMTCT_EID", worksheet.Cells[i, 10].Value.ToString()));
            validation.Add(new XElement("TX_NEW", worksheet.Cells[i, 11].Value.ToString()));
            validation.Add(new XElement("TB_STAT", worksheet.Cells[i, 12].Value.ToString()));
            validation.Add(new XElement("TB_ART", worksheet.Cells[i, 13].Value.ToString()));
            validation.Add(new XElement("TX_TB", worksheet.Cells[i, 14].Value.ToString()));
            validation.Add(new XElement("TB_PREV", worksheet.Cells[i, 15].Value.ToString()));
            validation.Add(new XElement("TX_Curr", worksheet.Cells["E13"].Value.ToString()));
            summaries.Add(validation);

            i = 8;
            var concurrency_rate = new XElement("Concurrence_rate");
            concurrency_rate.Add(new XElement("HTC_TST", worksheet.Cells[i, 2].Value.ToString()));
            concurrency_rate.Add(new XElement("HTC_TST_Pos", worksheet.Cells[i, 3].Value.ToString()));
            concurrency_rate.Add(new XElement("HTC_Only", worksheet.Cells[i, 4].Value.ToString()));
            concurrency_rate.Add(new XElement("HTC_Pos", worksheet.Cells[i, 5].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_STAT", worksheet.Cells[i, 6].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_STAT_Pos", worksheet.Cells[i, 7].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_STAT_Knwpos", worksheet.Cells[i, 8].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_ART", worksheet.Cells[i, 9].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_EID", worksheet.Cells[i, 10].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_NEW", worksheet.Cells[i, 11].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_STAT", worksheet.Cells[i, 12].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_ART", worksheet.Cells[i, 13].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_TB", worksheet.Cells[i, 14].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_PREV", worksheet.Cells[i, 15].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_Curr", worksheet.Cells["E14"].Value.ToString()));
            summaries.Add(concurrency_rate);
 

            var summary_value = new dqa_summary_value();
            summary_value.metadata_id = medata_data_id;
            summary_value.summary_object = summaries.ToString();
            entity.dqa_summary_value.Add(summary_value);

            entity.SaveChanges();
        }

         
        /// <summary>
        /// this is a method to download the DQA tool
        /// </summary>
        /// <param name="facilities">< list of facility pivot table reported in DATIM/param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<string> GenerateDQA(List<PivotTableModel> facilities, Organizations ip)
        {
            string fileName = "SHIELD_DQA_Q3_" + ip.ShortName + ".zip";
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q3/SHIELD_DQA_Q3_" + ip.ShortName);
             
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

            var artSites = GetARTSite();
            

            //site name come from pivot table and names match the data in health facility in the database
            foreach (var site in facilities)
            {
                string radetSite;
                artSites.TryGetValue(site.FacilityCode, out radetSite);// artSites[site.FacilityCode];

                if (string.IsNullOrEmpty(radetSite))
                {
                    radetSite = site.FacilityName;
                }

                string template = System.Web.Hosting.HostingEnvironment.MapPath(System.Configuration.ConfigurationManager.AppSettings["DQAToolQ3"]);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
                {
                    string radetPeriod = System.Configuration.ConfigurationManager.AppSettings["ReportPeriod"];
                    var radet = new RadetMetaDataDAO().RetrieveRadetLineListingForDQA(radetPeriod, site.IP, radetSite);// site.FacilityName);
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
                            sheet.Cells["V" + row].Value = radet[i].CurrentViralLoad;
                            sheet.Cells["W" + row].Value = radet[i].DateOfCurrentViralLoad; // string.Format("{0:d-MMM-yyyy}", radet[i].DateOfCurrentViralLoad);
                            sheet.Cells["W" + row].Style.Numberformat.Format = "d-MMM-yyyy";
                            sheet.Cells["X" + row].Value = radet[i].ViralLoadIndication;
                            sheet.Cells["Y" + row].Value = radet[i].CurrentARTStatus;
                        }
                        
                    }

                    var sheetn = package.Workbook.Worksheets["Worksheet"];
                    sheetn.Cells["P2"].Value = site.IP;
                    sheetn.Cells["R2"].Value = site.State;
                    sheetn.Cells["T2"].Value = site.Lga;
                    sheetn.Cells["V2"].Value = site.FacilityName;
                    sheetn.Cells["AA2"].Value = site.FacilityCode;
                    sheetn.Cells["Y2"].Value = "Q3 FY17";

                    sheetn.Cells["O10"].Value = radet.Count;

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

                    var sheet__all_Summary = package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"];
                    sheet__all_Summary.Cells["E12"].Value = radet.Count;

                    package.SaveAs(new FileInfo(directory + "/" + site.FacilityName + ".xlsm"));
                }
            }

            await ZipFolder(directory);
            return fileName;
        }

        private async Task ZipFolder(string filepath)
        {
            string[] filenames = Directory.GetFiles(filepath);

            using (ZipOutputStream s = new
            ZipOutputStream(File.Create(filepath + ".zip")))
            {
                s.SetLevel(9); // 0-9, 9 being the highest compression

                byte[] buffer = new byte[4096];

                foreach (string file in filenames)
                {

                    ZipEntry entry = new
                    ZipEntry(Path.GetFileName(file));

                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = await fs.ReadAsync(buffer, 0,
                            buffer.Length);

                            s.Write(buffer, 0, sourceBytes);

                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
        }

        private async Task ZipFiles(Dictionary<string, MemoryStream> tools, string filepath)
        {
            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
            zipStream.SetLevel(9); //0-9, 9 being the highest level of compression
            foreach (var t in tools)
            {
                ZipEntry entry = new ZipEntry(t.Key + ".xlsm");
                entry.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry);
                StreamUtils.Copy(t.Value, zipStream, new byte[4096]);
                zipStream.CloseEntry();
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            outputMemStream.Position = 0;

            using (var fileStream = File.Create(filepath))
            {
                await outputMemStream.CopyToAsync(fileStream);
            }
        }

        public List<dqaQ1Fy17Analysis> GetAnalysisReport()
        {
            var _analysis = entity.dqa_FY17Q1_Analysyis.ToList();
            List<dqaQ1Fy17Analysis> t = (from item in _analysis
                                         select new dqaQ1Fy17Analysis
                                         {
                                             IP = item.IP,
                                             Facility = item.Facility,

                                             DQA_FY17Q1_HTC_TST = item.DQA_FY17Q1_HTC_TST,
                                             Validated_HTC_TST = item.Validated_HTC_TST,
                                             CR_HTC_TST = item.CR_HTC_TST.HasValue ? (item.CR_HTC_TST.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_PMTCT_STAT = item.DQA_FY17Q1_PMTCT_STAT,
                                             Validated_PMTCT_STAT = item.Validated_PMTCT_STAT,
                                             CR_PMTCT_STAT = item.CR_PMTCT_STAT.HasValue ? (item.CR_PMTCT_STAT.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_PMTCT_EID = item.DQA_FY17Q1_PMTCT_EID,
                                             Validated_PMTCT_EID = item.Validated_PMTCT_EID,
                                             CR_PMTCT_EID = item.CR_PMTCT_EID.HasValue ? (item.CR_PMTCT_EID.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_PMTCT_ARV = item.DQA_FY17Q1_PMTCT_ARV,
                                             Validated_PMTCT_ARV = item.Validated_PMTCT_ARV,
                                             CR_PMTCT_ARV = item.CR_PMTCT_ARV.HasValue ? (item.CR_PMTCT_ARV.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_TX_NEW = item.DQA_FY17Q1_TX_NEW,
                                             Validated_TX_NEW = item.Validated_TX_NEW,
                                             CR_TX_NEW = item.CR_TX_NEW.HasValue ? (item.CR_TX_NEW.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_TX_CURR = item.DQA_FY17Q1_TX_CURR,
                                             Validated_TX_CURR = item.Validated_TX_CURR,
                                             CR_TX_CURR = item.CR_TX_CURR.HasValue ? (item.CR_TX_CURR.Value * 100).ToString("N1") : " ",

                                             Count_Fails = item.Count_Fails
                                         }).ToList();
            return t;
        }

        public bool ReadPivotTable(Stream datimFile, string quarter, int year, Profile profile, out string result)
        {
            var hfs = entity.HealthFacilities.ToDictionary(x => x.FacilityCode);
            StringBuilder sb = new StringBuilder();

            List<dqa_pivot_table> pivotTable = new List<dqa_pivot_table>();
            try
            {
                using (ExcelPackage package = new ExcelPackage(datimFile))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
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
                            //Logger.LogInfo("pivot table upload", "unknown facility with code [" + fCode + "," + fName + "] uploaded ");
                            //throw new ApplicationException("unknown facility with code [" + fCode + "] uploaded ");
                        }


                        if (hf != null)
                        {
                            if (hf.ImplementingPartner.Id != profile.Organization.Id)
                            {
                                sb.AppendLine("Facility [" + hf.Name + "] does not belong to the your IP [" + profile.Organization.ShortName + "]. Please correct and try again");
                                //throw new ApplicationException("Facility [" + hf.Name + "] does not belong to the your IP [" + profile.Organization.ShortName + "]. Please correct and try again");
                            }

                            //if(hf.Name != fName)
                            //{
                            //    //sb.AppendLine(string.Format("Update dbo.HealthFacility set Name = '{0}' where id= {1}; ", fName, hf.Id));
                            //}

                            int tb_art = 0, ovc = 0;

                            int hts_tst = ExcelHelper.ReadCellText(worksheet, row, 3).ToInt();
                            int htc_only = ExcelHelper.ReadCellText(worksheet, row, 4).ToInt();
                            int htc_only_pos = ExcelHelper.ReadCellText(worksheet, row, 5).ToInt();
                            int pmtct_stat = ExcelHelper.ReadCellText(worksheet, row, 6).ToInt();
                            int pmtct_stat_new = ExcelHelper.ReadCellText(worksheet, row, 7).ToInt();
                            int pmtct_stat_prev = ExcelHelper.ReadCellText(worksheet, row, 8).ToInt();
                            int pmtct_art = ExcelHelper.ReadCellText(worksheet, row, 9).ToInt();
                            int pmtct_eid = ExcelHelper.ReadCellText(worksheet, row, 10).ToInt();
                            int tx_new = ExcelHelper.ReadCellText(worksheet, row, 11).ToInt();
                            int tx_curr = ExcelHelper.ReadCellText(worksheet, row, 12).ToInt();

                            pivotTable.Add(new dqa_pivot_table
                            {
                                HTS_TST = hts_tst,
                                PMTCT_EID = pmtct_eid,
                                HTC_Only = htc_only,
                                HTC_Only_POS = htc_only_pos,
                                PMTCT_STAT = pmtct_stat,
                                PMTCT_STAT_NEW = pmtct_stat_new,
                                PMTCT_STAT_PREV = pmtct_stat_prev,
                                TX_NEW = tx_new,
                                TB_ART = tb_art,
                                PMTCT_ART = pmtct_art,
                                TX_CURR = tx_curr,
                                OVC = ovc,
                                HealthFacility = hf,
                                Quarter = quarter,
                                ImplementingPartner = hf.ImplementingPartner,
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

                //delete previous submissions
                var previously = entity.dqa_pivot_table_upload.FirstOrDefault(x => x.Quarter == quarter.Trim() && x.ImplementingPartner.Id == profile.Organization.Id);
                if (previously != null)
                {
                    entity.dqa_pivot_table_upload.Remove(previously);
                }

                entity.dqa_pivot_table_upload.Add(
                    new dqa_pivot_table_upload
                    {
                        DateUploaded = DateTime.Now,
                        dqa_pivot_table = selectedList,
                        IP = profile.Organization.Id,
                        Quarter = quarter,
                        UploadedBy = profile.Id
                    });

                entity.dqa_pivot_table.RemoveRange(entity.dqa_pivot_table.Where(x => x.Quarter == quarter.Trim() && x.ImplementingPartner.Id == profile.Organization.Id).ToList());
                entity.dqa_pivot_table.AddRange(selectedList);
                entity.SaveChanges();

                result = Newtonsoft.Json.JsonConvert.SerializeObject(
                    from item in selectedList
                    select new
                    {
                        FacilityName = item.HealthFacility.Name,
                        item.OVC,
                        item.PMTCT_ART,
                        item.TB_ART,
                        item.TX_CURR,
                        item.PMTCT_STAT,
                        item.PMTCT_EID,
                        item.PMTCT_STAT_NEW,
                        item.PMTCT_STAT_PREV,
                        item.HTC_Only,
                        item.HTC_Only_POS,
                        item.HTS_TST,
                        item.TX_NEW,
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


        /// <summary>
        /// returns a dictionary of datim code and radet facility name
        /// </summary>
        /// <returns></returns>
       public static Dictionary<string, string> GetARTSite()
        {
            string art_file = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/ART sites.xlsx");
            //code and radet name
            Dictionary<string, string> artSites = new Dictionary<string, string>();
            using (var package = new ExcelPackage(new FileInfo(art_file)))
            {
                var aSheet = package.Workbook.Worksheets.FirstOrDefault();
                int row = 2;
                while (true)
                {
                    string r_name = ExcelHelper.ReadCellText(aSheet, row, 2);
                    if (string.IsNullOrEmpty(r_name))
                        break;
                    string d_code = ExcelHelper.ReadCellText(aSheet, row, 4);
                    if (string.IsNullOrEmpty(d_code.Trim()))
                        throw new ApplicationException("no code found");

                    if (d_code.Trim().Contains("#N/A") == false)
                        artSites.Add(d_code, r_name.Trim());
                    row++;
                }
            } 
            return artSites;
        }
    }
}
