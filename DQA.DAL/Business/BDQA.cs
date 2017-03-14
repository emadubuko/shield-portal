using CommonUtil.Utilities;
using DQA.DAL.Data;
using OfficeOpenXml;
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

        public string ReadWorkbook(string filename,string username)
        {
            using (ExcelPackage package=new ExcelPackage(new FileInfo(filename)))
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

                    return "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td>" + filename + " was processed successfully</td></tr>";
                }
                catch(Exception ex)
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
        private void ReadSummary(ExcelWorksheet worksheet,int medata_data_id)
        {

            var sections = new[] { "Completeness", "Consistency", "Precision", "Integrity", "Validity" };
            
            
            for (var i = 8; i < 19; i++)
            {
                var indicator_code = worksheet.Cells[i, 1].Value.ToString();
                var indicator = entity.dqa_summary_indicators.FirstOrDefault(e => e.summary_code == indicator_code);
                var summaries = new XElement("summaries");
                summaries.Add(new XElement("datim_report", worksheet.Cells[i, 19].Value.ToString()));
                summaries.Add(new XElement("concurrence_rate", worksheet.Cells[i, 20].Value.ToString()));
                summaries.Add(new XElement("indicator_id", indicator.id));

                var summary = new XElement("summary_1");
                summary.Add(new XElement("month", worksheet.Cells["D6"].Value.ToString()));
                var col_id = 4;
                for (var j = 0; j < 5; j++)
                {
                    var value = "";
                    if (worksheet.Cells[i, (j + col_id)].Value != null)
                    {
                        value = worksheet.Cells[i, (j + col_id)].Value.ToString();
                    }
                    summary.Add(new XElement(sections[j],value ));
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
                        value = worksheet.Cells[i, (j + col_id)].Value.ToString();
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
                        value = worksheet.Cells[i, (j + col_id)].Value.ToString();
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

            var template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/NEW DQA TEMPLATE.xlsm");
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
            throw new ApplicationException("Facility code does not exist in the source document");
        }

        [Obsolete]
        private string RetrieveDatimData(string uri)
        {
            string script = null; 

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("UMB_SHIELD:UMB@sh1eld")));
                httpClient.Timeout = TimeSpan.FromMinutes(2);
                script = httpClient.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                script = script.Split(new string[] { "rows", "width" }, StringSplitOptions.RemoveEmptyEntries)[1];
                script = script.Replace(":", "").Replace("[[", "").Replace("]],", "").Replace("\"", "");
                var t = script.Split(new string[] { ",", }, StringSplitOptions.None);
                script = t[t.Count() - 1];

                //await httpClient.GetAsync(uri)
                //   .ContinueWith(x =>
                //   {
                //       if (x.IsCompleted && x.Status == TaskStatus.RanToCompletion)
                //       {
                //          script = x.Result.Content.ReadAsStringAsync().Result.Split(new string[] { "rows", "width" }, StringSplitOptions.RemoveEmptyEntries)[1];
                //           script = script.Replace(":", "").Replace("[[", "").Replace("]],", "").Replace("\"", "");
                //           var t = script.Split(new string[] { ",", }, StringSplitOptions.None);
                //           script = t[t.Count() - 1];                            
                //      }
                //   });
            }
            return script;
        }


        //delete reports of a particular metadataId
        public void Delete(int metadataId)
        {
           var report_values= entity.dqa_report_value.Where(e=>e.MetadataId==metadataId);
            entity.dqa_report_value.RemoveRange(report_values);

            entity.dqa_report_metadata.Remove(entity.dqa_report_metadata.Find(metadataId));
            entity.SaveChanges();

        }
        
    }
}