using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using DQA.DAL.Model;
using OfficeOpenXml;
using RADET.DAL.DAO;

namespace DQA.DAL.Business
{
    public class BDQAQ3FY18
    {
        shield_dmpEntities entity;

        public BDQAQ3FY18()
        {
            entity = new shield_dmpEntities();
        }

        public string ReadWorkbook(string filename, Profile Theuser)
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
                    if (Theuser.Organization.Id != facility.ImplementingPartnerId
                        && Theuser.RoleName == "ip")
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> This facility <b>" + facility.Name + "</b> is not mapped to your Organization. </td></tr>";
                    }

                    metadata.AssessmentWeek = 1;
                    metadata.CreateDate = DateTime.Now;
                    metadata.CreatedBy = Theuser.ContactEmailAddress;
                    metadata.FiscalYear = DateTime.Now.Year.ToString();
                    metadata.FundingAgency = 1;
                    metadata.ImplementingPartner = facility.ImplementingPartnerId.Value;
                    metadata.LgaId = facility.LGAId;
                    metadata.LgaLevel = 2;
                    metadata.ReportPeriod = "Q3 FY18"; //worksheet.Cells["Y2"].Value.ToString();
                    metadata.SiteId = Convert.ToInt32(facility.Id);
                    metadata.StateId = facility.lga.state.state_code; // state.state_code;

                    var domain = Theuser.ContactEmailAddress.Split('@')[1];

                    var meta_counts = entity.dqa_report_metadata
                        .Where(e => e.ReportPeriod == metadata.ReportPeriod
                        && e.ImplementingPartner == metadata.ImplementingPartner
                        && e.SiteId == metadata.SiteId);// && e.CreatedBy.Contains(domain));

                    foreach (var p_item in meta_counts)
                    {
                        var thatUserRole = new CommonUtil.DAO.ProfileDAO().GetRoleByEmail(p_item.CreatedBy);
                        if (thatUserRole == Theuser.RoleName)
                        {
                            return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. Report already exists in the database</td></tr>";
                        }
                    }
                    entity.dqa_report_metadata.Add(metadata);
                    entity.SaveChanges();

                    worksheet = package.Workbook.Worksheets["All Questions"];

                    //get all the indicators in the system
                    var indicators = entity.dqa_indicator.Where(x => x.DQAPeriod == metadata.ReportPeriod);

                    for (var i = 7; i < 59; i++)
                    {
                        if (worksheet.Cells[i, 2].Value == null)
                            continue;
                        var indicator_code = worksheet.Cells[i, 2].Value.ToString();
                        var indicator = indicators.FirstOrDefault(e => e.IndicatorCode == indicator_code);
                        if (indicator == null)
                            continue;

                        var istValueMonth = worksheet.Cells[i, 4];
                        var sndValueMonth = worksheet.Cells[i, 5];
                        var trdValueMonth = worksheet.Cells[i, 6];
                        var question = worksheet.Cells[i, 2];

                        //check if there is a value for the indicator
                        if ((istValueMonth == null || istValueMonth.Value == null) &&
                            (sndValueMonth == null || sndValueMonth.Value == null) &&
                            (trdValueMonth == null || trdValueMonth.Value == null) && question == null || question.Value == null)
                            continue;

                        if (istValueMonth.Value == null)
                            istValueMonth.Value = 0;
                        if (sndValueMonth.Value == null)
                            sndValueMonth.Value = 0;
                        if (trdValueMonth.Value == null)
                            trdValueMonth.Value = 0;

                        int z = 0;
                        if (!int.TryParse(istValueMonth.Value.ToString(), out z) || !int.TryParse(sndValueMonth.Value.ToString(), out z) || !int.TryParse(trdValueMonth.Value.ToString(), out z))
                        {

                            entity.dqa_report_metadata.Remove(metadata);
                            entity.SaveChanges();
                            return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>Non numeric entry found in the validation field. <br /> Ensure the file does not have circular reference error before uploading <br /> File :" + new FileInfo(filename).Name + "  </td></tr>";
                        }

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

                    return "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td>" + new FileInfo(filename).Name + " was processed successfully</td></tr>";
                }
                catch (DbEntityValidationException e)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        sb.AppendLine(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                        foreach (var ve in eve.ValidationErrors)
                        {
                            sb.AppendLine(string.Format("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage));
                        }
                    }
                    Logger.LogInfo("BDQAQ3FY18.ReadWorkbook", sb.ToString());
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>System error has occurred while processing the file " + new FileInfo(filename).Name + "</td></tr>";
                }
                catch (Exception ex)
                {
                    entity.dqa_report_metadata.Remove(metadata);
                    entity.SaveChanges();
                    Logger.LogError(ex);
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>There are errors " + new FileInfo(filename).Name + "</td></tr>";
                }
            }
        }

        private void ReadSummary(ExcelWorksheet summaryworksheet, int medata_data_id)
        {
            int i = 6;
            var summaries = new XElement("summaries");
            var reported_data = new XElement("reported_data");
            reported_data.Add(new XElement("HTC_TST", summaryworksheet.Cells[i, 2].Value.ToString()));
            reported_data.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
            reported_data.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
            reported_data.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_STAT", summaryworksheet.Cells[i, 6].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_STAT_Knwpos", summaryworksheet.Cells[i, 8].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_HEI_POS", summaryworksheet.Cells[i, 11].Value.ToString()));
            reported_data.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 12].Value.ToString()));
            reported_data.Add(new XElement("TX_CURR", summaryworksheet.Cells["E12"].Value.ToString()));

            reported_data.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 13].Value.ToString()));
            reported_data.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 14].Value.ToString()));
            reported_data.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 15].Value.ToString()));
            summaries.Add(reported_data);

            i = 7;
            var validation = new XElement("validation");
            validation.Add(new XElement("HTC_TST", summaryworksheet.Cells[i, 2].Value.ToString()));
            validation.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
            validation.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
            validation.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT", summaryworksheet.Cells[i, 6].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT_Knwpos", summaryworksheet.Cells[i, 8].Value.ToString()));
            validation.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
            validation.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
            validation.Add(new XElement("PMTCT_HEI_POS", summaryworksheet.Cells[i, 11].Value.ToString()));
            validation.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 12].Value.ToString()));
            validation.Add(new XElement("TX_CURR", summaryworksheet.Cells["E13"].Value.ToString()));

            validation.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 13].Value.ToString()));
            validation.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 14].Value.ToString()));
            validation.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 15].Value.ToString()));
            summaries.Add(validation);

            i = 8;
            var concurrency_rate = new XElement("Concurrence_rate");
            concurrency_rate.Add(new XElement("HTC_TST", summaryworksheet.Cells[i, 2].Value.ToString()));
            concurrency_rate.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
            concurrency_rate.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
            concurrency_rate.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_STAT", summaryworksheet.Cells[i, 6].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_STAT_Knwpos", summaryworksheet.Cells[i, 8].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_HEI_POS", summaryworksheet.Cells[i, 11].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 12].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_CURR", summaryworksheet.Cells["E14"].Value.ToString()));

            concurrency_rate.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 13].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 14].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 15].Value.ToString()));
            summaries.Add(concurrency_rate);
             
            var summary_value = new dqa_summary_value();
            summary_value.metadata_id = medata_data_id;
            summary_value.summary_object = summaries.ToString();
            entity.dqa_summary_value.Add(summary_value);

            entity.SaveChanges();
        }
         
        public bool ReadPivotTable(Stream uploadedFile, string reportPeriod, Profile profile, out string result)
        {
            var previously = entity.dqa_pivot_table_upload.FirstOrDefault(x => x.Quarter == reportPeriod.Trim() && x.ImplementingPartner.Id == profile.Organization.Id);
            if (previously != null)
            {
                result = "Pivot table already uploaded";
                return false;
            }

            var hfs = entity.HealthFacilities.ToDictionary(x => x.FacilityCode);
            StringBuilder sb = new StringBuilder();

            List<dqa_pivot_table> pivotTable = new List<dqa_pivot_table>();
            try
            {
                using (ExcelPackage package = new ExcelPackage(uploadedFile))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(); //["Facility Pivot Table"];
                    if (worksheet == null)
                    {
                        throw new ApplicationException("Invalid pivot table uploaded");
                    }

                    int row = 2;

                    while (true)
                    {
                        string fCode = ExcelHelper.ReadCell(worksheet, row, 1);
                        string fName = ExcelHelper.ReadCellText(worksheet, row, 2);
                        if (string.IsNullOrEmpty(fCode))
                        {
                            break;
                        }
                        if (hfs.TryGetValue(fCode, out Data.HealthFacility hf) == false)
                        {
                            sb.AppendLine("unknown facility with code [" + fCode + "," + fName + "] uploaded ");
                        }
                        if (hf != null)
                        {
                            if (hf.ImplementingPartner.Id != profile.Organization.Id)
                            {
                                sb.AppendLine("Facility [" + hf.Name + "] does not belong to the your IP [" + profile.Organization.ShortName + "]. Please correct and try again");
                                break;
                            }

                            int hts_tst = ExcelHelper.ReadCellText(worksheet, row, 3).ToInt();
                            int htc_only = ExcelHelper.ReadCellText(worksheet, row, 4).ToInt();
                            int htc_only_pos = ExcelHelper.ReadCellText(worksheet, row, 5).ToInt();
                            int pmtct_stat = ExcelHelper.ReadCellText(worksheet, row, 6).ToInt();
                            int pmtct_stat_new = ExcelHelper.ReadCellText(worksheet, row, 7).ToInt();
                            int pmtct_stat_prev = ExcelHelper.ReadCellText(worksheet, row, 8).ToInt();
                            int pmtct_art = ExcelHelper.ReadCellText(worksheet, row, 9).ToInt();
                            int pmtct_eid = ExcelHelper.ReadCellText(worksheet, row, 10).ToInt();
                            int pmtct_hei_pos = ExcelHelper.ReadCellText(worksheet, row, 11).ToInt();
                            int tx_new = ExcelHelper.ReadCellText(worksheet, row, 12).ToInt();
                            int tx_curr = ExcelHelper.ReadCellText(worksheet, row, 13).ToInt();
                            int tb_stat = ExcelHelper.ReadCellText(worksheet, row, 14).ToInt();
                            int tb_art = ExcelHelper.ReadCellText(worksheet, row, 15).ToInt();
                            int tx_tb = ExcelHelper.ReadCellText(worksheet, row, 16).ToInt();

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
                                PMTCT_HEI_POS = pmtct_hei_pos,
                                TX_NEW = tx_new,
                                TX_CURR = tx_curr,
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
                }

                if (pivotTable.Count() == 0)
                {
                    sb.AppendLine("No valid record found in the pivot table. " + Environment.NewLine + "Ensure that the 'organisationunitid and organisationunitname' columns are populated");
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
                        if (selectedList.Where(x=>x.SelectedForDQA).Sum(x => (x.TX_CURR + x.PMTCT_ART)) <= _80_percent_of_Total_tx_curr)
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
                        item.PMTCT_ART,
                        item.PMTCT_EID,
                        item.PMTCT_HEI_POS,
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
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Logger.LogError(ex);
                return false;
            }
            return true;
        }
    }
}
