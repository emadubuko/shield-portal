using DQA.DAL.Data;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DQA.DAL.Business
{
    public class BDQA
    {
        shield_dmpEntities entity = new shield_dmpEntities();

        public string ReadWorkbook(string filename,string username)
        {
            using (ExcelPackage package=new ExcelPackage(new FileInfo(filename)))
            {

                var worksheet = package.Workbook.Worksheets["Worksheet"];
                //var metaSheet = package.Workbook.Worksheets["CDC DQA"];
                var excel_value = worksheet.Cells["O3"].Value.ToString();
                var partner = entity.ImplementingPartners.FirstOrDefault(e => e.ShortName == excel_value);
                if (partner==null)
                {
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. The partner does not exist.</td></tr>";
                }
                excel_value = worksheet.Cells["Q3"].Value.ToString();
                var state = entity.dqa_states.FirstOrDefault(e => e.state_name ==excel_value);
                if (state == null)
                {
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. State is incorrect</td></tr>";
                }

                excel_value = worksheet.Cells["S3"].Value.ToString().Split()[1];

                var lga = entity.dqa_lga.FirstOrDefault(e => e.lga_name == excel_value);
                if (lga == null)
                {
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. The LGA is incorrect</td></tr>";
                }
                excel_value = worksheet.Cells["S3"].Value.ToString();
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
                metadata.ImplementingPartner = partner.Id;
                metadata.LgaId = lga.id;
                metadata.LgaLevel = 2;
                metadata.ReportPeriod = worksheet.Cells["Q8"].Value.ToString();
                metadata.SiteId = Convert.ToInt32(facility.Id);//worksheet.Cells["Z3"].Value.ToString();
                metadata.StateId = state.id;

                //var worksheet = package.Workbook.Worksheets["Data entry"];

                //check if the report exists
                var meta = entity.dqa_report_metadata.Where(e => e.FiscalYear == metadata.FiscalYear && e.FundingAgency == metadata.FundingAgency && e.ReportPeriod == metadata.ReportPeriod && e.ImplementingPartner == metadata.ImplementingPartner && e.SiteId == metadata.SiteId);
                if (meta.Any())
                {
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename+ " could not be processed. Report already exists in the database</td></tr>";
                }
                entity.dqa_report_metadata.Add(metadata);
                entity.SaveChanges();


                worksheet = package.Workbook.Worksheets["Data entry"];
                //get all the indicators in the system
                var indicators = entity.dqa_indicator;
                //var cells = worksheet.Cells[1, 1, 945, 3];
                for(var i = 4; i < 956; i++)
                {
                    var value = worksheet.Cells[i, 3];
                    //check if there is a value for the indicator
                    if (value == null || value.Value ==null || worksheet.Cells[i, 4]==null || worksheet.Cells[i, 5].Value==null || worksheet.Cells[i, 1].Value==null)
                        continue;

                    var indicator_code = worksheet.Cells[i, 1].Value.ToString();
                    var indicator = indicators.FirstOrDefault(e => e.IndicatorCode == indicator_code);
                    var report_value = new dqa_report_value();
                    report_value.MetadataId = metadata.Id;
                    report_value.IndicatorId = indicator.Id;
                    report_value.IndicatorValueMonth1 = Utility.GetDecimal(worksheet.Cells[i, 3].Value);//Convert.ToInt32(value.Value);
                    report_value.IndicatorValueMonth2 = Utility.GetDecimal(worksheet.Cells[i, 4].Value);
                    report_value.IndicatorValueMonth3 = Utility.GetDecimal(worksheet.Cells[i, 5].Value);

                    entity.dqa_report_value.Add(report_value);
                   
                }
                entity.SaveChanges();
                return "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td>" + filename + "was processed successfully</td></tr>";
            }
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
                var worksheet = package.Workbook.Worksheets["Data entry"];
                for(var i = 4; i < 946; i++)
                {
                    //check if there is a value for the indicator
                    if (worksheet.Cells[i, 1] == null || worksheet.Cells[i, 1].Value == null)
                        continue;
                    var indicator_code = worksheet.Cells[i, 1].Value.ToString();
                    var report_value = report_values.FirstOrDefault(e => e.dqa_indicator.IndicatorCode == indicator_code);
                    if (report_value == null) continue;
                    worksheet.Cells[i, 3].Value = report_value.IndicatorValueMonth1;
                    worksheet.Cells[i, 4].Value = report_value.IndicatorValueMonth2;
                    worksheet.Cells[i, 5].Value = report_value.IndicatorValueMonth3;

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