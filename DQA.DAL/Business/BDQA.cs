using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using DQA.DAL.Model;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace DQA.DAL.Business
{
    public class BDQA
    {
        shield_dmpEntities entity = new shield_dmpEntities();

        public string ReadWorkbook(string filename, string username, string[] datimNumbersRaw)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filename)))
            {
                try
                {
                    var worksheet = package.Workbook.Worksheets["Worksheet"];
                    //var metaSheet = package.Workbook.Worksheets["CDC DQA"];
                    var excel_value = worksheet.Cells["P2"].Value.ToString();
                    //var partner = entity.ImplementingPartners.FirstOrDefault(e => e.ShortName == excel_value);
                    //if (partner == null)
                    //{
                    //    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. The partner does not exist.</td></tr>";
                    //}
                    excel_value = worksheet.Cells["R2"].Value.ToString();
                    var state = entity.states.FirstOrDefault(e => e.state_name == excel_value);
                    if (state == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. State is incorrect</td></tr>";
                    }

                    excel_value = worksheet.Cells["T2"].Value.ToString().Substring(3, worksheet.Cells["T2"].Value.ToString().Length - 3);

                    //var lga = entity.lgas.FirstOrDefault(e => e.lga_name == excel_value);
                    //if (lga == null)
                    //{
                    //    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. The LGA is incorrect</td></tr>";
                    //}



                    excel_value = worksheet.Cells["AA2"].Value.ToString();
                    var facility = entity.HealthFacilities.FirstOrDefault(e => e.FacilityCode == excel_value);

                    if (facility == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. The facility is incorrect</td></tr>";
                    }

                    //get the metadata of the report
                    var metadata = new dqa_report_metadata();
                    metadata.AssessmentWeek = 1;//Convert.ToInt32(worksheet.Cells["Q8"].Value.ToString());
                    metadata.CreateDate = DateTime.Now;
                    metadata.CreatedBy = username;
                    metadata.FiscalYear = DateTime.Now.Year.ToString();
                    metadata.FundingAgency = 1;
                    metadata.ImplementingPartner = facility.ImplementingPartnerId.Value;
                    metadata.LgaId = facility.LGAId;
                    metadata.LgaLevel = 2;
                    metadata.ReportPeriod = worksheet.Cells["Y2"].Value.ToString();
                    metadata.SiteId = Convert.ToInt32(facility.Id);//worksheet.Cells["Z3"].Value.ToString();
                    metadata.StateId = state.state_code;

                    //var worksheet = package.Workbook.Worksheets["Data entry"];

                    //check if the report exists
                    var meta = entity.dqa_report_metadata.Where(e => e.FiscalYear == metadata.FiscalYear && e.FundingAgency == metadata.FundingAgency && e.ReportPeriod == metadata.ReportPeriod && e.ImplementingPartner == metadata.ImplementingPartner && e.SiteId == metadata.SiteId);
                    if (meta.Any())
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. Report already exists in the database</td></tr>";
                    }
                    entity.dqa_report_metadata.Add(metadata);
                    entity.SaveChanges();


                    worksheet = package.Workbook.Worksheets["All Questions"];
                    //get all the indicators in the system
                    var indicators = entity.dqa_indicator;
                    //var cells = worksheet.Cells[1, 1, 945, 3];
                    for (var i = 8; i < 205; i++)
                    {
                        var value = worksheet.Cells[i, 4];
                        //check if there is a value for the indicator
                        if (value == null || value.Value == null || worksheet.Cells[i, 5] == null || worksheet.Cells[i, 6].Value == null || worksheet.Cells[i, 2].Value == null)
                            continue;

                        var indicator_code = worksheet.Cells[i, 2].Value.ToString();
                        var indicator = indicators.FirstOrDefault(e => e.IndicatorCode == indicator_code);
                        if (indicator == null)
                            continue;
                        var report_value = new dqa_report_value();
                        report_value.MetadataId = metadata.Id;
                        report_value.IndicatorId = indicator.Id;
                        report_value.IndicatorValueMonth1 = Utility.GetDecimal(worksheet.Cells[i, 4].Value);//Convert.ToInt32(value.Value);
                        report_value.IndicatorValueMonth2 = Utility.GetDecimal(worksheet.Cells[i, 5].Value);
                        report_value.IndicatorValueMonth3 = Utility.GetDecimal(worksheet.Cells[i, 6].Value);

                        entity.dqa_report_value.Add(report_value);

                    }
                    entity.SaveChanges();

                    ReadSummary(package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"], metadata.Id);

                    SaveDQADimensions(package.Workbook, metadata.Id);

                    SaveDQAComparison(package.Workbook, metadata.Id, datimNumbersRaw);

                    return "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td>" + filename + " was processed successfully</td></tr>";
                }
                catch (Exception ex)
                {
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

            var sections = new[] { "Completeness", "Consistency", "Precision", "Integrity", "Validity" };


            for (var i = 8; i < 19; i++)
            {
                var indicator_code = worksheet.Cells[i, 1].Value.ToString();
                var indicator = entity.dqa_summary_indicators.FirstOrDefault(e => e.summary_code == indicator_code);
                var summaries = new XElement("summaries");
                summaries.Add(new XElement("datim_report", ExcelHelper.ReadCell(worksheet, i, 19)));  //worksheet.Cells[i, 19].Value.ToString()));

                decimal concurence = 0;
                decimal.TryParse(ExcelHelper.ReadCell(worksheet, i, 20), out concurence);
                summaries.Add(new XElement("concurrence_rate", concurence * 100)); // worksheet.Cells[i, 20].Value.ToString()));
                summaries.Add(new XElement("indicator_id", indicator.id));

                var summary = new XElement("summary_1");
                summary.Add(new XElement("month", worksheet.Cells["D6"].Value.ToString()));
                var col_id = 4;
                for (var j = 0; j < 5; j++)
                {
                    var value = "";
                    if (worksheet.Cells[i, (j + col_id)].Value != null)
                    {
                        value = ExcelHelper.ReadCell(worksheet, i, (j + col_id)); // worksheet.Cells[i, (j + col_id)].Value.ToString();
                    }
                    summary.Add(new XElement(sections[j], value));
                }
                summaries.Add(summary);


                summary = new XElement("summary_2");
                summary.Add(new XElement("month", worksheet.Cells["I6"].Value.ToString()));
                col_id = 9;
                for (var j = 0; j < 5; j++)
                {
                    //summary.Add(new XElement(sections[j], worksheet.Cells[i, (j + col_id)].Value.ToString()));
                    var value = "";
                    if (worksheet.Cells[i, (j + col_id)].Value != null)
                    {
                        value = ExcelHelper.ReadCell(worksheet, i, (j + col_id));// worksheet.Cells[i, (j + col_id)].Value.ToString();
                    }
                    summary.Add(new XElement(sections[j], value));
                }
                summaries.Add(summary);


                summary = new XElement("summary_3");
                summary.Add(new XElement("month", worksheet.Cells["N6"].Value.ToString()));
                col_id = 14;
                for (var j = 0; j < 5; j++)
                {
                    // summary.Add(new XElement(sections[j], worksheet.Cells[i, (j + col_id)].Value.ToString()));
                    var value = "";
                    if (worksheet.Cells[i, (j + col_id)].Value != null)
                    {
                        value = ExcelHelper.ReadCell(worksheet, i, (j + col_id)); // worksheet.Cells[i, (j + col_id)].Value.ToString();
                    }
                    summary.Add(new XElement(sections[j], value));
                }
                summaries.Add(summary);

                var summary_value = new dqa_summary_value();
                summary_value.metadata_id = medata_data_id;
                summary_value.summary_object = summaries.ToString();
                entity.dqa_summary_value.Add(summary_value);
            }

            entity.SaveChanges();
        }

        /// <summary>
        /// Load a report back to a workook
        /// </summary>
        /// <param name="metadataId"></param>
        /// <returns></returns>
        public string LoadWorkbook(int metadataId, string[] datimNumbersRaw)
        {
            string output = "";

            string Datim_HTC_TST = "";
            string DATIM_HTC_TST_POS = "";
            string DATIM_HTC_ONLY = "";
            string DATIM_HTC_POS = "";
            string DATIM_PMTCT_STAT = "";
            string DATIM_PMTCT_STAT_POS = "";
            string DATIM_PMTCT_STAT_Previously = "";
            string DATIM_PMTCT_EID = "";
            string DATIM_PMTCT_ART = "";
            string Datim_TX_NEW = "";
            string DATIM_TX_CURR = "";

            string HTC_TST = "";
            string HTC_TST_POS = "";
            string HTC_ONLY = "";
            string HTC_POS = "";
            string PMTCT_STAT = "";
            string PMTCT_STAT_POS = "";
            string PMTCT_STAT_Previoulsy_Known = "";
            string PMTCT_EID = "";
            string PMTCT_ART = "";
            string TX_NEW = "";
            string TX_Curr = "";


            var meta = entity.dqa_report_metadata.FirstOrDefault(e => e.Id == metadataId);
            var facility = entity.HealthFacilities.FirstOrDefault(e => e.Id == meta.SiteId); //(e => e.Id == meta.Id);

            string FacilityName = facility.Name;
            string FacilityCode = facility.FacilityCode;

            var template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/SHIELD_DQA_v2_20170321.xlsm");
            var newfile = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Downloads/" + facility.Name + ".xlsm");
            var report_values = entity.dqa_report_value.Where(e => e.MetadataId == metadataId && e.dqa_indicator.Readonly == "");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
            {
                var worksheet = package.Workbook.Worksheets["Worksheet"];
                worksheet.Cells["AA2"].Value = facility.FacilityCode;
                worksheet.Cells["V2"].Value = facility.Name;

                var partner = entity.ImplementingPartners.FirstOrDefault(e => e.Id == meta.ImplementingPartner);
                worksheet.Cells["P2"].Value = partner.ShortName;

                var state = entity.states.FirstOrDefault(e => e.state_code == meta.StateId);
                worksheet.Cells["R2"].Value = state.state_name;

                var lga = entity.lgas.FirstOrDefault(e => e.lga_code == meta.LgaId);
                worksheet.Cells["T2"].Value = lga.lga_name;

                worksheet.Cells["Y2"].Value = "Q1(Oct - Dec)";

                worksheet = package.Workbook.Worksheets["All Questions"];
                for (var i = 8; i < 205; i++)
                {
                    //check if there is a value for the indicator
                    if (worksheet.Cells[i, 2] == null || worksheet.Cells[i, 2].Value == null)
                        continue;
                    var indicator_code = worksheet.Cells[i, 2].Value.ToString();
                    var report_value = report_values.FirstOrDefault(e => e.dqa_indicator.IndicatorCode == indicator_code);
                    if (report_value == null)
                        continue;
                    worksheet.Cells[i, 4].Value = report_value.IndicatorValueMonth1;
                    worksheet.Cells[i, 5].Value = report_value.IndicatorValueMonth2;
                    worksheet.Cells[i, 6].Value = report_value.IndicatorValueMonth3;
                }

                string datimNumberLine = SearchMasterList(datimNumbersRaw, FacilityCode);
                string[] datimNumber = datimNumberLine.Split(',');

                Datim_HTC_TST = datimNumber[15];
                DATIM_HTC_TST_POS = "";
                DATIM_HTC_ONLY = !string.IsNullOrEmpty(datimNumber[15]) && !string.IsNullOrEmpty(datimNumber[9]) ? (Convert.ToInt32(datimNumber[15]) - Convert.ToInt32(datimNumber[9])).ToString() : " ";
                DATIM_HTC_POS = "";
                DATIM_PMTCT_STAT = datimNumber[9];
                DATIM_PMTCT_STAT_POS = "";
                DATIM_PMTCT_STAT_Previously = "";
                DATIM_PMTCT_EID = datimNumber[14];
                DATIM_PMTCT_ART = datimNumber[12];
                Datim_TX_NEW = datimNumber[7];
                DATIM_TX_CURR = datimNumber[4];

                worksheet.Cells["F2"].Value = !string.IsNullOrEmpty(Datim_HTC_TST) ? Convert.ToInt32(Datim_HTC_TST) : 0;
                worksheet.Cells["F46"].Value = !string.IsNullOrEmpty(DATIM_PMTCT_STAT) ? Convert.ToInt32(DATIM_PMTCT_STAT) : 0;
                worksheet.Cells["F91"].Value = !string.IsNullOrEmpty(DATIM_PMTCT_ART) ? Convert.ToInt32(DATIM_PMTCT_ART) : 0;
                worksheet.Cells["F141"].Value = !string.IsNullOrEmpty(Datim_TX_NEW) ? Convert.ToInt32(Datim_TX_NEW) : 0;
                worksheet.Cells["F166"].Value = !string.IsNullOrEmpty(DATIM_TX_CURR) ? Convert.ToInt32(DATIM_TX_CURR) : 0;
                worksheet.Cells["F117"].Value = !string.IsNullOrEmpty(DATIM_PMTCT_EID) ? Convert.ToInt32(DATIM_PMTCT_EID) : 0;

                //read the calculated score
                worksheet = package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"];
                HTC_TST = ExcelHelper.ReadCell(worksheet, 8, 22);
                HTC_TST_POS = ExcelHelper.ReadCell(worksheet, 9, 22);
                HTC_ONLY = ExcelHelper.ReadCell(worksheet, 10, 22);
                HTC_POS = ExcelHelper.ReadCell(worksheet, 11, 22);
                PMTCT_STAT = ExcelHelper.ReadCell(worksheet, 12, 22);
                PMTCT_STAT_POS = ExcelHelper.ReadCell(worksheet, 13, 22);
                PMTCT_STAT_Previoulsy_Known = ExcelHelper.ReadCell(worksheet, 14, 22);
                PMTCT_EID = ExcelHelper.ReadCell(worksheet, 15, 22);
                PMTCT_ART = ExcelHelper.ReadCell(worksheet, 16, 22);
                TX_NEW = ExcelHelper.ReadCell(worksheet, 17, 22);
                TX_Curr = ExcelHelper.ReadCell(worksheet, 18, 22);

                #region mem burner
                /*
                string HTC_TST = string.Format("https://www.datim.org/api/analytics?dimension=ou:{0}&dimension=dx:K6f6jR0NOcZ&filter=pe:2016Q4&displayProperty=NAME&hierarchyMeta=true&showHierarchy=true&rows=ou;dx", facility.FacilityCode);
                string PMTCT_STAT = string.Format("https://www.datim.org/api/analytics?dimension=ou:{0}&dimension=dx:nO2Z2HmcFpc&filter=pe:2016Q4&displayProperty=NAME&hierarchyMeta=true&showHierarchy=true&rows=ou;dx", facility.FacilityCode);
                string PMTCT_ART = string.Format("https://www.datim.org/api/analytics?dimension=ou:{0}&dimension=dx:wPfGMcoz1Z0&filter=pe:2016Q4&displayProperty=NAME&hierarchyMeta=true&showHierarchy=true&rows=ou;dx", facility.FacilityCode);
                string TX_NEW = string.Format("https://www.datim.org/api/analytics?dimension=ou:{0}&dimension=dx:tG7ocyZ8kVA&filter=pe:2016Q4&displayProperty=NAME&hierarchyMeta=true&showHierarchy=true&rows=ou;dx", facility.FacilityCode);
                string TX_CURR = string.Format("https://www.datim.org/api/analytics?dimension=ou:{0}&dimension=dx:OuudMtJsh2z&filter=pe:2016Q4&displayProperty=NAME&hierarchyMeta=true&showHierarchy=true&rows=ou;dx", facility.FacilityCode);
                string PMTCT_EID = string.Format("https://www.datim.org/api/analytics?dimension=ou:{0}&dimension=dx:vet57h5gZvw&filter=pe:2016Q4&displayProperty=NAME&hierarchyMeta=true&showHierarchy=true&rows=ou;dx", facility.FacilityCode);
                worksheet.Cells["F2"].Value = RetrieveDatimData(HTC_TST);
                worksheet.Cells["F46"].Value = RetrieveDatimData(PMTCT_STAT);
                worksheet.Cells["F91"].Value = RetrieveDatimData(PMTCT_ART);
                worksheet.Cells["F141"].Value = RetrieveDatimData(TX_NEW);
                worksheet.Cells["F166"].Value = RetrieveDatimData(TX_CURR);
                worksheet.Cells["F117"].Value = RetrieveDatimData(PMTCT_EID);
                */
                #endregion
                package.SaveAs(new FileInfo(newfile));
            }

            output = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}",
                   FacilityName.Replace(",", "-"), FacilityCode, Datim_HTC_TST, HTC_TST, DATIM_HTC_TST_POS, HTC_TST_POS, DATIM_HTC_ONLY, HTC_ONLY, DATIM_HTC_POS, HTC_POS, DATIM_PMTCT_STAT, PMTCT_STAT, DATIM_PMTCT_STAT_POS, PMTCT_STAT_POS, DATIM_PMTCT_STAT_Previously, PMTCT_STAT_Previoulsy_Known, DATIM_PMTCT_EID, PMTCT_EID, DATIM_PMTCT_ART, PMTCT_ART, Datim_TX_NEW, TX_NEW, DATIM_TX_CURR, TX_Curr);
            return output;
        }

        private string SearchMasterList(string[] lines, string fcode)
        {
            foreach (var line in lines)
            {
                string[] items = line.Split(',');
                if (string.IsNullOrEmpty(items[0]))
                    break;

                if (items[0].Trim() == fcode)
                    return line;
            }
            throw new ApplicationException("Facility code " + fcode + " does not exist in the source document");
        }

        public void GenerateDQADimensionsFromDB(string dimension_outputfile, string comparsion_outputfile)
        {
            StringBuilder sb_dimensions = new StringBuilder();
            sb_dimensions.AppendLine("FacilityName, FacilityCode, HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts, Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID, PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW, TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT, Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr, HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions, Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID, PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions, Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR, Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID, Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr, Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID, Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr");
            var dqadimensions = entity.dqa_dimensions.ToList();

            foreach (var dimensions in dqadimensions)
            {
                sb_dimensions.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43}",
                       dimensions.FacilityName.Replace(",", "-"), dimensions.FacilityCode, dimensions.HTC_Charts, dimensions.Total_Completeness_HTC_TST, dimensions.PMTCT_STAT_charts, dimensions.Total_Completeness_PMTCT_STAT, dimensions.PMTCT_EID_charts, dimensions.Total_completeness_PMTCT_EID, dimensions.PMTCT_ARV_Charts, dimensions.Total_completeness_PMTCT_ARV, dimensions.TX_NEW_charts, dimensions.Total_completeness_TX_NEW, dimensions.TX_CURR_charts, dimensions.Total_completeness_TX_CURR, dimensions.Total_consistency_HTC_TST, dimensions.Total_consistency_PMTCT_STAT, dimensions.Total_consistency_PMTCT_EID, dimensions.Total_consistency_PMTCT_ART, dimensions.Total_consistency_TX_NEW, dimensions.Total_consistency_TX_Curr, dimensions.HTC_Charts_Precisions, dimensions.Total_precision_HTC_TST, dimensions.PMTCT_STAT_Charts_Precisions, dimensions.Total_precision_PMTCT_STAT, dimensions.PMTCT_EID_Charts_Precisions, dimensions.Total_precision_PMTCT_EID, dimensions.PMTCT_ARV_Charts_Precisions, dimensions.Total_precision_PMTCT_ARV, dimensions.TX_NEW_Charts_Precisions, dimensions.Total_precision_TX_NEW, dimensions.TX_CURR_Charts_Precisions, dimensions.Total_precision_TX_CURR, dimensions.Total_integrity_HTC_TST, dimensions.Total_integrity_PMTCT_STAT, dimensions.Total_integrity_PMTCT_EID, dimensions.Total_integrity_PMTCT_ART, dimensions.Total_integrity_TX_NEW, dimensions.Total_integrity_TX_Curr, dimensions.Total_Validity_HTC_TST, dimensions.Total_Validity_PMTCT_STAT, dimensions.Total_Validity_PMTCT_EID, dimensions.Total_Validity_PMTCT_ART, dimensions.Total_Validity_TX_NEW, dimensions.Total_Validity_TX_Curr));
            }
            File.WriteAllText(dimension_outputfile, sb_dimensions.ToString());

            StringBuilder sb_comparison = new StringBuilder();
            sb_comparison.AppendLine("facility name,facility code, datim_htc_tst, htc_tst, datim_htc_tst_pos, htc_tst_pos, datim_htc_only, htc_only, datim_htc_pos, htc_pos, datim_pmtct_stat, pmtct_stat, datim_pmtct_stat_pos, pmtct_stat_pos, datim_pmtct_stat_previously, pmtct_stat_previoulsy_known, datim_pmtct_eid,pmtct_eid,datim_pmtct_art,pmtct_art,datim_tx_new,tx_new,datim_tx_curr,tx_curr");
            var dqaComparsion = entity.dqa_comparison.ToList();
            foreach (var item in dqaComparsion)
            {
                sb_comparison.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}",
                   item.FacilityName.Replace(",", "-"), item.FacilityCode, item.Datim_HTC_TST, item.HTC_TST, item.DATIM_HTC_TST_POS, item.HTC_TST_POS, item.DATIM_HTC_ONLY, item.HTC_ONLY, item.DATIM_HTC_POS, item.HTC_POS, item.DATIM_PMTCT_STAT, item.PMTCT_STAT, item.DATIM_PMTCT_STAT_POS, item.PMTCT_STAT_POS, item.DATIM_PMTCT_STAT_Previously, item.PMTCT_STAT_Previoulsy_Known, item.DATIM_PMTCT_EID, item.PMTCT_EID, item.DATIM_PMTCT_ART, item.PMTCT_ART, item.Datim_TX_NEW, item.TX_NEW, item.DATIM_TX_CURR, item.TX_Curr));
            }
            File.WriteAllText(comparsion_outputfile, sb_comparison.ToString());
        }


        [Obsolete]
        public void GenerateDQADimensionsFromFile(string outputfile)
        {
            string baseLocation = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Downloads/");
            string[] files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("FacilityName, FacilityCode, HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts, Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID, PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW, TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT, Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr, HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions, Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID, PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions, Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR, Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID, Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr, Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID, Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr");
            var dqadimensions = new List<dqa_dimensions>();
            foreach (var file in files)
            {
                string FacilityName = "", FacilityCode = "";
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    package.Workbook.Worksheets.ToList().ForEach(w => w.Hidden = eWorkSheetHidden.Visible);


                    ExcelWorksheet sht = package.Workbook.Worksheets["Worksheet"];

                    FacilityName = ExcelHelper.ReadCell(sht, 2, 22);
                    FacilityCode = ExcelHelper.ReadCell(sht, 2, 27);

                    sht = package.Workbook.Worksheets["All Questions"];

                    string HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts,
                    Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID,
                    PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW,
                    TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT,
                    Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr,
                    HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions,
                    Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID,
                    PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions,
                    Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR,
                    Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID,
                    Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr,
                    Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID,
                    Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr;

                    //HTC_Charts = ExcelHelper.ReadCell(sht, 6, 6);
                    //PMTCT_STAT_charts = ExcelHelper.ReadCell(sht, 49, 6);
                    //PMTCT_EID_charts = ExcelHelper.ReadCell(sht, 117, 6);
                    //PMTCT_ARV_Charts = ExcelHelper.ReadCell(sht, 91, 6);
                    //TX_NEW_charts = ExcelHelper.ReadCell(sht, 142, 6);
                    //TX_CURR_charts = ExcelHelper.ReadCell(sht, 167, 6);

                    HTC_Charts_Precisions = ExcelHelper.ReadCell(sht, 2, 6);
                    HTC_Charts = ExcelHelper.GetRandomizeChartNUmber(HTC_Charts_Precisions);

                    PMTCT_STAT_Charts_Precisions = ExcelHelper.ReadCell(sht, 46, 6);
                    PMTCT_STAT_charts = ExcelHelper.GetRandomizeChartNUmber(PMTCT_STAT_Charts_Precisions);

                    PMTCT_EID_Charts_Precisions = ExcelHelper.ReadCell(sht, 117, 6);
                    PMTCT_EID_charts = ExcelHelper.GetRandomizeChartNUmber(PMTCT_EID_Charts_Precisions);

                    PMTCT_ARV_Charts_Precisions = ExcelHelper.ReadCell(sht, 91, 6);
                    PMTCT_ARV_Charts = ExcelHelper.GetRandomizeChartNUmber(PMTCT_ARV_Charts_Precisions);

                    TX_NEW_Charts_Precisions = ExcelHelper.ReadCell(sht, 141, 6);
                    TX_NEW_charts = ExcelHelper.GetRandomizeChartNUmber(TX_NEW_Charts_Precisions);

                    TX_CURR_Charts_Precisions = ExcelHelper.ReadCell(sht, 166, 6);
                    TX_CURR_charts = ExcelHelper.GetRandomizeChartNUmber(TX_CURR_Charts_Precisions);

                    sht = package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"];

                    Total_Completeness_HTC_TST = ExcelHelper.ReadCell(sht, 8, 23);
                    Total_Completeness_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 23);
                    Total_completeness_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 23);
                    Total_completeness_PMTCT_ARV = ExcelHelper.ReadCell(sht, 16, 23);
                    Total_completeness_TX_NEW = ExcelHelper.ReadCell(sht, 17, 23);
                    Total_completeness_TX_CURR = ExcelHelper.ReadCell(sht, 18, 23);

                    Total_consistency_HTC_TST = ExcelHelper.ReadCell(sht, 8, 24);
                    Total_consistency_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 24);
                    Total_consistency_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 24);
                    Total_consistency_PMTCT_ART = ExcelHelper.ReadCell(sht, 16, 24);
                    Total_consistency_TX_NEW = ExcelHelper.ReadCell(sht, 17, 24);
                    Total_consistency_TX_Curr = ExcelHelper.ReadCell(sht, 18, 24);

                    Total_precision_HTC_TST = ExcelHelper.ReadCell(sht, 8, 25);
                    Total_precision_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 25);
                    Total_precision_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 25);
                    Total_precision_PMTCT_ARV = ExcelHelper.ReadCell(sht, 16, 25);
                    Total_precision_TX_NEW = ExcelHelper.ReadCell(sht, 17, 25);
                    Total_precision_TX_CURR = ExcelHelper.ReadCell(sht, 18, 25);

                    Total_integrity_HTC_TST = ExcelHelper.ReadCell(sht, 8, 26);
                    Total_integrity_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 26);
                    Total_integrity_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 26);
                    Total_integrity_PMTCT_ART = ExcelHelper.ReadCell(sht, 16, 26);
                    Total_integrity_TX_NEW = ExcelHelper.ReadCell(sht, 17, 26);
                    Total_integrity_TX_Curr = ExcelHelper.ReadCell(sht, 18, 26);

                    Total_Validity_HTC_TST = ExcelHelper.ReadCell(sht, 8, 27);
                    Total_Validity_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 27);
                    Total_Validity_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 27);
                    Total_Validity_PMTCT_ART = ExcelHelper.ReadCell(sht, 16, 27);
                    Total_Validity_TX_NEW = ExcelHelper.ReadCell(sht, 17, 27);
                    Total_Validity_TX_Curr = ExcelHelper.ReadCell(sht, 18, 27);

                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43}",
                        FacilityName.Replace(",", "-"), FacilityCode, HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts, Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID, PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW, TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT, Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr, HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions, Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID, PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions, Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR, Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID, Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr, Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID, Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr));

                    var aDqaDimension = new dqa_dimensions
                    {
                        FacilityCode = FacilityCode,
                        FacilityName = FacilityName,
                        HTC_Charts_Precisions = HTC_Charts_Precisions,
                        HTC_Charts = HTC_Charts,
                        PMTCT_ARV_Charts = PMTCT_ARV_Charts,
                        PMTCT_ARV_Charts_Precisions = PMTCT_ARV_Charts_Precisions,
                        PMTCT_EID_charts = PMTCT_EID_charts,
                        PMTCT_EID_Charts_Precisions = PMTCT_EID_Charts_Precisions,
                        PMTCT_STAT_charts = PMTCT_STAT_charts,
                        PMTCT_STAT_Charts_Precisions = PMTCT_STAT_Charts_Precisions,
                        Total_Completeness_HTC_TST = Total_Completeness_HTC_TST,
                        Total_completeness_PMTCT_ARV = Total_completeness_PMTCT_ARV,
                        Total_completeness_PMTCT_EID = Total_completeness_PMTCT_EID,
                        Total_Completeness_PMTCT_STAT = Total_Completeness_PMTCT_STAT,
                        Total_completeness_TX_CURR = Total_completeness_TX_CURR,
                        Total_completeness_TX_NEW = Total_completeness_TX_NEW,
                        Total_consistency_HTC_TST = Total_consistency_HTC_TST,
                        Total_consistency_PMTCT_ART = Total_consistency_PMTCT_ART,
                        Total_consistency_PMTCT_EID = Total_consistency_PMTCT_EID,
                        Total_consistency_PMTCT_STAT = Total_consistency_PMTCT_STAT,
                        Total_consistency_TX_Curr = Total_consistency_TX_Curr,
                        Total_consistency_TX_NEW = Total_consistency_TX_NEW,
                        Total_integrity_HTC_TST = Total_integrity_HTC_TST,
                        Total_integrity_PMTCT_ART = Total_integrity_PMTCT_ART,
                        Total_integrity_PMTCT_EID = Total_integrity_PMTCT_EID,
                        Total_integrity_PMTCT_STAT = Total_integrity_PMTCT_STAT,
                        Total_integrity_TX_Curr = Total_integrity_TX_Curr,
                        Total_integrity_TX_NEW = Total_integrity_TX_NEW,
                        Total_precision_HTC_TST = Total_precision_HTC_TST,
                        Total_precision_PMTCT_ARV = Total_precision_PMTCT_ARV,
                        Total_precision_PMTCT_EID = Total_precision_PMTCT_EID,
                        Total_precision_PMTCT_STAT = Total_precision_PMTCT_STAT,
                        Total_precision_TX_CURR = Total_precision_TX_CURR,
                        Total_precision_TX_NEW = Total_precision_TX_NEW,
                        Total_Validity_HTC_TST = Total_Validity_HTC_TST,
                        Total_Validity_PMTCT_ART = Total_Validity_PMTCT_ART,
                        Total_Validity_PMTCT_EID = Total_Validity_PMTCT_EID,
                        Total_Validity_PMTCT_STAT = Total_Validity_PMTCT_STAT,
                        Total_Validity_TX_Curr = Total_Validity_TX_Curr,
                        Total_Validity_TX_NEW = Total_Validity_TX_NEW,
                        TX_CURR_charts = TX_CURR_charts,
                        TX_CURR_Charts_Precisions = TX_CURR_Charts_Precisions,
                        TX_NEW_charts = TX_NEW_charts,
                        TX_NEW_Charts_Precisions = TX_NEW_Charts_Precisions
                    };
                    dqadimensions.Add(aDqaDimension);
                }
            }
            File.WriteAllText(outputfile, sb.ToString());

            entity.dqa_dimensions.RemoveRange(entity.dqa_dimensions);
            entity.dqa_dimensions.AddRange(dqadimensions);

            entity.SaveChanges();
        }


        private void SaveDQADimensions(ExcelWorkbook workbook, int metatdata)
        {
            ExcelWorksheet sht = workbook.Worksheets["Worksheet"];
            string FacilityName = ExcelHelper.ReadCell(sht, 2, 22);
            string FacilityCode = ExcelHelper.ReadCell(sht, 2, 27);

            sht = workbook.Worksheets["All Questions"];

            string HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts,
            Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID,
            PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW,
            TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT,
            Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr,
            HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions,
            Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID,
            PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions,
            Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR,
            Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID,
            Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr,
            Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID,
            Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr;

            HTC_Charts_Precisions = ExcelHelper.ReadCell(sht, 2, 6);
            HTC_Charts = ExcelHelper.GetRandomizeChartNUmber(HTC_Charts_Precisions);

            PMTCT_STAT_Charts_Precisions = ExcelHelper.ReadCell(sht, 46, 6);
            PMTCT_STAT_charts = ExcelHelper.GetRandomizeChartNUmber(PMTCT_STAT_Charts_Precisions);

            PMTCT_EID_Charts_Precisions = ExcelHelper.ReadCell(sht, 117, 6);
            PMTCT_EID_charts = ExcelHelper.GetRandomizeChartNUmber(PMTCT_EID_Charts_Precisions);

            PMTCT_ARV_Charts_Precisions = ExcelHelper.ReadCell(sht, 91, 6);
            PMTCT_ARV_Charts = ExcelHelper.GetRandomizeChartNUmber(PMTCT_ARV_Charts_Precisions);

            TX_NEW_Charts_Precisions = ExcelHelper.ReadCell(sht, 141, 6);
            TX_NEW_charts = ExcelHelper.GetRandomizeChartNUmber(TX_NEW_Charts_Precisions);

            TX_CURR_Charts_Precisions = ExcelHelper.ReadCell(sht, 166, 6);
            TX_CURR_charts = ExcelHelper.GetRandomizeChartNUmber(TX_CURR_Charts_Precisions);

            sht = workbook.Worksheets["DQA Summary (Map to Quest Ans)"];

            Total_Completeness_HTC_TST = ExcelHelper.ReadCell(sht, 8, 23);
            Total_Completeness_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 23);
            Total_completeness_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 23);
            Total_completeness_PMTCT_ARV = ExcelHelper.ReadCell(sht, 16, 23);
            Total_completeness_TX_NEW = ExcelHelper.ReadCell(sht, 17, 23);
            Total_completeness_TX_CURR = ExcelHelper.ReadCell(sht, 18, 23);

            Total_consistency_HTC_TST = ExcelHelper.ReadCell(sht, 8, 24);
            Total_consistency_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 24);
            Total_consistency_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 24);
            Total_consistency_PMTCT_ART = ExcelHelper.ReadCell(sht, 16, 24);
            Total_consistency_TX_NEW = ExcelHelper.ReadCell(sht, 17, 24);
            Total_consistency_TX_Curr = ExcelHelper.ReadCell(sht, 18, 24);

            Total_precision_HTC_TST = ExcelHelper.ReadCell(sht, 8, 25);
            Total_precision_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 25);
            Total_precision_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 25);
            Total_precision_PMTCT_ARV = ExcelHelper.ReadCell(sht, 16, 25);
            Total_precision_TX_NEW = ExcelHelper.ReadCell(sht, 17, 25);
            Total_precision_TX_CURR = ExcelHelper.ReadCell(sht, 18, 25);

            Total_integrity_HTC_TST = ExcelHelper.ReadCell(sht, 8, 26);
            Total_integrity_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 26);
            Total_integrity_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 26);
            Total_integrity_PMTCT_ART = ExcelHelper.ReadCell(sht, 16, 26);
            Total_integrity_TX_NEW = ExcelHelper.ReadCell(sht, 17, 26);
            Total_integrity_TX_Curr = ExcelHelper.ReadCell(sht, 18, 26);

            Total_Validity_HTC_TST = ExcelHelper.ReadCell(sht, 8, 27);
            Total_Validity_PMTCT_STAT = ExcelHelper.ReadCell(sht, 12, 27);
            Total_Validity_PMTCT_EID = ExcelHelper.ReadCell(sht, 15, 27);
            Total_Validity_PMTCT_ART = ExcelHelper.ReadCell(sht, 16, 27);
            Total_Validity_TX_NEW = ExcelHelper.ReadCell(sht, 17, 27);
            Total_Validity_TX_Curr = ExcelHelper.ReadCell(sht, 18, 27);

            var aDqaDimension = new dqa_dimensions
            {
                FacilityCode = FacilityCode,
                FacilityName = FacilityName,
                HTC_Charts_Precisions = HTC_Charts_Precisions,
                HTC_Charts = HTC_Charts,
                PMTCT_ARV_Charts = PMTCT_ARV_Charts,
                PMTCT_ARV_Charts_Precisions = PMTCT_ARV_Charts_Precisions,
                PMTCT_EID_charts = PMTCT_EID_charts,
                PMTCT_EID_Charts_Precisions = PMTCT_EID_Charts_Precisions,
                PMTCT_STAT_charts = PMTCT_STAT_charts,
                PMTCT_STAT_Charts_Precisions = PMTCT_STAT_Charts_Precisions,
                Total_Completeness_HTC_TST = Total_Completeness_HTC_TST,
                Total_completeness_PMTCT_ARV = Total_completeness_PMTCT_ARV,
                Total_completeness_PMTCT_EID = Total_completeness_PMTCT_EID,
                Total_Completeness_PMTCT_STAT = Total_Completeness_PMTCT_STAT,
                Total_completeness_TX_CURR = Total_completeness_TX_CURR,
                Total_completeness_TX_NEW = Total_completeness_TX_NEW,
                Total_consistency_HTC_TST = Total_consistency_HTC_TST,
                Total_consistency_PMTCT_ART = Total_consistency_PMTCT_ART,
                Total_consistency_PMTCT_EID = Total_consistency_PMTCT_EID,
                Total_consistency_PMTCT_STAT = Total_consistency_PMTCT_STAT,
                Total_consistency_TX_Curr = Total_consistency_TX_Curr,
                Total_consistency_TX_NEW = Total_consistency_TX_NEW,
                Total_integrity_HTC_TST = Total_integrity_HTC_TST,
                Total_integrity_PMTCT_ART = Total_integrity_PMTCT_ART,
                Total_integrity_PMTCT_EID = Total_integrity_PMTCT_EID,
                Total_integrity_PMTCT_STAT = Total_integrity_PMTCT_STAT,
                Total_integrity_TX_Curr = Total_integrity_TX_Curr,
                Total_integrity_TX_NEW = Total_integrity_TX_NEW,
                Total_precision_HTC_TST = Total_precision_HTC_TST,
                Total_precision_PMTCT_ARV = Total_precision_PMTCT_ARV,
                Total_precision_PMTCT_EID = Total_precision_PMTCT_EID,
                Total_precision_PMTCT_STAT = Total_precision_PMTCT_STAT,
                Total_precision_TX_CURR = Total_precision_TX_CURR,
                Total_precision_TX_NEW = Total_precision_TX_NEW,
                Total_Validity_HTC_TST = Total_Validity_HTC_TST,
                Total_Validity_PMTCT_ART = Total_Validity_PMTCT_ART,
                Total_Validity_PMTCT_EID = Total_Validity_PMTCT_EID,
                Total_Validity_PMTCT_STAT = Total_Validity_PMTCT_STAT,
                Total_Validity_TX_Curr = Total_Validity_TX_Curr,
                Total_Validity_TX_NEW = Total_Validity_TX_NEW,
                TX_CURR_charts = TX_CURR_charts,
                TX_CURR_Charts_Precisions = TX_CURR_Charts_Precisions,
                TX_NEW_charts = TX_NEW_charts,
                TX_NEW_Charts_Precisions = TX_NEW_Charts_Precisions,
                MetadataId = metatdata
            };
            entity.dqa_dimensions.Add(aDqaDimension);
            entity.SaveChanges();
        }

        public void SaveDQAComparison(ExcelWorkbook workbook, int metadataId, string[] datimNumbersRaw)
        {
            dqa_comparison comparsion = new dqa_comparison();

            var meta = entity.dqa_report_metadata.FirstOrDefault(e => e.Id == metadataId);
            var facility = entity.HealthFacilities.FirstOrDefault(e => e.Id == meta.SiteId); //(e => e.Id == meta.Id);

            comparsion.FacilityName = facility.Name;
            comparsion.FacilityCode = facility.FacilityCode;

            string datimNumberLine = SearchMasterList(datimNumbersRaw, comparsion.FacilityCode);
            string[] datimNumber = datimNumberLine.Split(',');

            comparsion.Datim_HTC_TST = datimNumber[15];
            comparsion.DATIM_HTC_TST_POS = "";
            comparsion.DATIM_HTC_ONLY = !string.IsNullOrEmpty(datimNumber[15]) && !string.IsNullOrEmpty(datimNumber[9]) ? (Convert.ToInt32(datimNumber[15]) - Convert.ToInt32(datimNumber[9])).ToString() : " ";
            comparsion.DATIM_HTC_POS = "";
            comparsion.DATIM_PMTCT_STAT = datimNumber[9];
            comparsion.DATIM_PMTCT_STAT_POS = "";
            comparsion.DATIM_PMTCT_STAT_Previously = "";
            comparsion.DATIM_PMTCT_EID = datimNumber[14];
            comparsion.DATIM_PMTCT_ART = datimNumber[12];
            comparsion.Datim_TX_NEW = datimNumber[7];
            comparsion.DATIM_TX_CURR = datimNumber[4];

            //read the calculated score
            var worksheet = workbook.Worksheets["DQA Summary (Map to Quest Ans)"];
            comparsion.HTC_TST = ExcelHelper.ReadCell(worksheet, 8, 22);
            comparsion.HTC_TST_POS = ExcelHelper.ReadCell(worksheet, 9, 22);
            comparsion.HTC_ONLY = ExcelHelper.ReadCell(worksheet, 10, 22);
            comparsion.HTC_POS = ExcelHelper.ReadCell(worksheet, 11, 22);
            comparsion.PMTCT_STAT = ExcelHelper.ReadCell(worksheet, 12, 22);
            comparsion.PMTCT_STAT_POS = ExcelHelper.ReadCell(worksheet, 13, 22);
            comparsion.PMTCT_STAT_Previoulsy_Known = ExcelHelper.ReadCell(worksheet, 14, 22);
            comparsion.PMTCT_EID = ExcelHelper.ReadCell(worksheet, 15, 22);
            comparsion.PMTCT_ART = ExcelHelper.ReadCell(worksheet, 16, 22);
            comparsion.TX_NEW = ExcelHelper.ReadCell(worksheet, 17, 22);
            comparsion.TX_Curr = ExcelHelper.ReadCell(worksheet, 18, 22);

            comparsion.metadataId = metadataId;
            entity.dqa_comparison.Add(comparsion);
            entity.SaveChanges();
        }

        //delete reports of a particular metadataId
        public void Delete(int metadataId)
        {
            var report_values = entity.dqa_report_value.Where(e => e.MetadataId == metadataId).ToList();
            entity.dqa_report_value.RemoveRange(report_values);

            var mt = entity.dqa_report_metadata.FirstOrDefault(e => e.Id == metadataId);
            if (mt != null)
                entity.dqa_report_metadata.Remove(mt);

            var dqa_summary = entity.dqa_summary_value.Where(s => s.metadata_id == metadataId).ToList();
            if (dqa_summary != null)
                entity.dqa_summary_value.RemoveRange(dqa_summary);

            var dqadimension = entity.dqa_dimensions.Where(x => x.MetadataId == metadataId).ToList();
            if (dqadimension != null)
                entity.dqa_dimensions.RemoveRange(dqadimension);

            var dqacomparison = entity.dqa_comparison.Where(x => x.metadataId == metadataId).ToList();
            if (dqacomparison != null)
                entity.dqa_comparison.RemoveRange(dqacomparison);

            entity.SaveChanges();
        }
         
    }
}