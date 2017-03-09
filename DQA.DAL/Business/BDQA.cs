using CommonUtil.Utilities;
using DQA.DAL.Data;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string LoadWorkbook(int metadataId)
        {
            var template = "~/Report/Template/DQA Template v0.134.xlsx";
            var newfile = "~/Report/Downloads/file.xlsx";
            var report_values = entity.dqa_report_value.Where(e => e.MetadataId == metadataId && e.dqa_indicator.Readonly =="");
            using (ExcelPackage package=new ExcelPackage(new FileInfo(template),new FileInfo(newfile)))
            {
                var meta = entity.dqa_report_metadata.FirstOrDefault(e=>e.Id==metadataId);
                var facility = entity.HealthFacilities.FirstOrDefault(e => e.Id == meta.Id);
                var worksheet = package.Workbook.Worksheets["Worksheet"];
                worksheet.Cells["AA2"].Value = facility.FacilityCode;

                var partner = entity.ImplementingPartners.FirstOrDefault(e => e.Id == meta.ImplementingPartner);
                worksheet.Cells["P2"].Value = partner.ShortName;


                var state = entity.states.FirstOrDefault(e => e.state_code == meta.StateId);
                worksheet.Cells["R2"].Value = state.state_name;
                




                worksheet = package.Workbook.Worksheets["All Questions"];
                for(var i = 8; i < 205; i++)
                {
                    //check if there is a value for the indicator
                    if (worksheet.Cells[i, 1] == null || worksheet.Cells[i, 1].Value == null)
                        continue;
                    var indicator_code = worksheet.Cells[i, 1].Value.ToString();
                    var report_value = report_values.FirstOrDefault(e => e.dqa_indicator.IndicatorCode == indicator_code);
                    if (report_value == null) continue;
                    worksheet.Cells[i, 4].Value = report_value.IndicatorValueMonth1;
                    worksheet.Cells[i, 5].Value = report_value.IndicatorValueMonth2;
                    worksheet.Cells[i, 6].Value = report_value.IndicatorValueMonth3;

                }
               
            }

            return newfile;
            
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