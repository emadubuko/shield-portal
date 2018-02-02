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
using System.Data.Entity.Validation;
using System.Xml.Linq;

namespace DQA.DAL.Business
{
    public class BDQAQ1FY18
    {
        shield_dmpEntities entity;

        public BDQAQ1FY18()
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
                    metadata.ReportPeriod = worksheet.Cells["Y2"].Value.ToString();
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

                    ReadSummary(worksheet, package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"], metadata.Id);

                    return "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td>" + filename + " was processed successfully</td></tr>";
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
                    Logger.LogInfo("BDQAQ2.ReadWorkbook", sb.ToString());
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>System error has occurred while processing the file " + filename + "</td></tr>";
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
        /// <param name="summaryworksheet"></param>
        /// <param name="medata_data_id"></param>
        private void ReadSummary(ExcelWorksheet allQuestionworksheet, ExcelWorksheet summaryworksheet, int medata_data_id)
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
            reported_data.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
            reported_data.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
            reported_data.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
            reported_data.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
            reported_data.Add(new XElement("PMTCT_FO", summaryworksheet.Cells[i, 15].Value.ToString()));
            reported_data.Add(new XElement("TX_Curr", summaryworksheet.Cells["E12"].Value.ToString()));
            reported_data.Add(new XElement("TX_RET", summaryworksheet.Cells["J12"].Value.ToString()));
            reported_data.Add(new XElement("TX_PLVS", summaryworksheet.Cells["P12"].Value.ToString()));
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
            validation.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
            validation.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
            validation.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
            validation.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
            validation.Add(new XElement("PMTCT_FO", summaryworksheet.Cells[i, 15].Value.ToString()));
            validation.Add(new XElement("TX_Curr", summaryworksheet.Cells["E13"].Value.ToString()));
            reported_data.Add(new XElement("TX_RET", summaryworksheet.Cells["J13"].Value.ToString()));
            reported_data.Add(new XElement("TX_PLVS", summaryworksheet.Cells["P13"].Value.ToString()));
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
            concurrency_rate.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
            concurrency_rate.Add(new XElement("PMTCT_FO", summaryworksheet.Cells[i, 15].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_Curr", summaryworksheet.Cells["E14"].Value.ToString()));
            reported_data.Add(new XElement("TX_RET", summaryworksheet.Cells["J14"].Value.ToString()));
            reported_data.Add(new XElement("TX_PLVS", summaryworksheet.Cells["P14"].Value.ToString()));

            summaries.Add(concurrency_rate);


            var summary_value = new dqa_summary_value();
            summary_value.metadata_id = medata_data_id;
            summary_value.summary_object = summaries.ToString();
            entity.dqa_summary_value.Add(summary_value);

            entity.SaveChanges();

            #region -oldcode
            /*
            int pmtct_stat = allQuestionworksheet.Cells["D16"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["E16"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["F16"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["D17"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["E17"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["F17"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["D18"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["E18"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["F18"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["D20"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["E20"].Value.ToString().ToInt();
            pmtct_stat += allQuestionworksheet.Cells["F20"].Value.ToString().ToInt();
            double pmtct_stat_value = Math.Round((pmtct_stat * 1.0) / 2, MidpointRounding.AwayFromZero);

            int htc_stat = allQuestionworksheet.Cells["D7"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["E7"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["F7"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["D8"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["E8"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["F8"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["D9"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["E9"].Value.ToString().ToInt();
            htc_stat += allQuestionworksheet.Cells["F9"].Value.ToString().ToInt();

            double htc_stat_value = Math.Round((htc_stat * 1.0) / 2, MidpointRounding.AwayFromZero);
            htc_stat_value = htc_stat_value + pmtct_stat_value;

            double PMTCT_STAT_Knwpos = 0;
            double.TryParse(summaryworksheet.Cells[i, 8].Value.ToString(), out PMTCT_STAT_Knwpos);

            htc_stat_value = htc_stat_value - PMTCT_STAT_Knwpos;


            i = 7;
            var validation = new XElement("validation");
            validation.Add(new XElement("HTC_TST", htc_stat_value)); //summaryworksheet.Cells[i, 2].Value.ToString()));
            validation.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
            validation.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
            validation.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));

            validation.Add(new XElement("PMTCT_STAT", pmtct_stat_value)); //summaryworksheet.Cells[i, 6].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
            validation.Add(new XElement("PMTCT_STAT_Knwpos", PMTCT_STAT_Knwpos)); //summaryworksheet.Cells[i, 8].Value.ToString()));
            validation.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
            validation.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
            validation.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
            validation.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
            validation.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
            validation.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
            validation.Add(new XElement("TB_PREV", summaryworksheet.Cells[i, 15].Value.ToString()));
            validation.Add(new XElement("TX_Curr", summaryworksheet.Cells["E13"].Value.ToString()));
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
            concurrency_rate.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
            concurrency_rate.Add(new XElement("TB_PREV", summaryworksheet.Cells[i, 15].Value.ToString()));
            concurrency_rate.Add(new XElement("TX_Curr", summaryworksheet.Cells["E14"].Value.ToString()));
            summaries.Add(concurrency_rate);


            var summary_value = new dqa_summary_value();
            summary_value.metadata_id = medata_data_id;
            summary_value.summary_object = summaries.ToString();
            entity.dqa_summary_value.Add(summary_value);

            entity.SaveChanges();
            */
            #endregion
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
                                continue;
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

                            int pmtct_hei_pos = ExcelHelper.ReadCellText(worksheet, row, 11).ToInt();

                            int tx_new = ExcelHelper.ReadCellText(worksheet, row, 12).ToInt();
                            int tx_curr = ExcelHelper.ReadCellText(worksheet, row, 13).ToInt();

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
                                HealthFacility = hf,
                                Quarter = reportPeriod,
                                ImplementingPartner = hf.ImplementingPartner,
                            });
                        }
                        row += 1;
                    }
                }

                //check for valid entry
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


        public string GetReportDetails(int metadataid)
        {
            var report_value = new dqa_report_value();
            var result = from item in entity.dqa_report_value.Where(x => x.MetadataId == metadataid)
                         select new
                         {
                             item.dqa_indicator.ThematicArea,
                             item.dqa_indicator.IndicatorName,
                             item.IndicatorValueMonth1,
                             item.IndicatorValueMonth2,
                             item.IndicatorValueMonth3
                         };
            string Processed_result = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return Processed_result;
        }


        /// <summary>
        /// this is a method to download the DQA tool
        /// </summary>
        /// <param name="facilities">< list of facility pivot table reported in DATIM/param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<string> GenerateDQA(List<PivotTableModel> facilities, Organizations ip)
        {
            string fileName = "DQA_Q1_" + ip.ShortName + ".zip";
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2018 Q1/DQA_Q1_" + ip.ShortName);

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

                string template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2018 Q1/FY18_Q1_Data Quality Assessment tool_v5.xlsm");
                using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
                {
                    var radet = new RadetMetaDataDAO().RetrieveRadetLineListingForDQA("Q1 FY18", site.IP, radetSite);// site.FacilityName);
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
                    }

                    var sheetn = package.Workbook.Worksheets["Worksheet"];
                    sheetn.Cells["P2"].Value = site.IP;
                    sheetn.Cells["R2"].Value = site.State;
                    sheetn.Cells["T2"].Value = site.Lga;
                    sheetn.Cells["V2"].Value = site.FacilityName;
                    sheetn.Cells["AA2"].Value = site.FacilityCode;
                    sheetn.Cells["Y2"].Value = "Q4 FY17";

                    sheetn.Cells["N10"].Value = tx_current_count;
                    //sheetn.Cells["X10"].Value = viral_load_count;
                    //sheetn.Cells["X11"].Value = viral_load_count_suppression;

                    //if (viral_load_count > 0)
                    //{
                    //    sheetn.Cells["X12"].Value = 1.0 * viral_load_count_suppression / viral_load_count;
                    //}



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
                    //PMTCT_HEI_POS
                    sheet__all_Q.Cells["F33"].Value = site.PMTCT_HEI_POS;
                    //TX_NEW
                    sheet__all_Q.Cells["F38"].Value = site.TX_NEW;

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
