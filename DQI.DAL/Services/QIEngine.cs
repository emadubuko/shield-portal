using CommonUtil.DAO;
using CommonUtil.Entities;
using DQI.DAL.DAO;
using DQI.DAL.Model;
using DQA.DAL.Data;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Data.Entity.Validation;
using System.Text;
using CommonUtil.Utilities;
using System.Xml.Linq;

namespace DQI.DAL.Services
{
    public class QIEngine
    {
        readonly shield_dmpEntities entities = new shield_dmpEntities();

        public DQIModel GetQISites(string reportPeriod, string ip = "")
        {
            var data = SQLUtility.GetSitesForDQI(reportPeriod, ip);
            var facilities = new HealthFacilityDAO().RetrieveAll();

            var dqiModel = new DQIModel();
            var sites = new List<DQISites>();

            foreach (DataRow row in data.Rows)
            {
                sites.Add(new DQISites
                {
                    IP = row[0].ToString(),
                    Facilty = facilities.FirstOrDefault(x => x.Id == Convert.ToInt32(row[2])),
                    Indicator = row[3].ToString(),
                    indicatorConcurrence = Convert.ToDouble(row[4]).ToString("N2"),
                    IndicatorDeviation = Convert.ToDouble(row[5]).ToString("N2"),
                    DATIM = Convert.ToDouble(row[6].ToString()),
                    Validated = Convert.ToDouble(row[7].ToString())
                });
            }

            dqiModel.DQISites = sites;

            var iPGrouping = sites.GroupBy(x => x.IP);
            var iplevel = new List<IPLevelDQI>();

            foreach (var group in iPGrouping)
            {
                var facility_indicator_grouping = group.ToList().GroupBy(x => x.Indicator);
                string indicator = "";
                int previousCount = 0;
                double concurrence = 0;
                foreach (var f in facility_indicator_grouping)
                {
                    if (f.ToList().Count() > previousCount)
                    {
                        previousCount = f.ToList().Count();
                        indicator = f.Key;
                        concurrence = 100 * f.ToList().Sum(x => x.Validated) / f.ToList().Sum(x => x.DATIM);
                    }
                }


                iplevel.Add(new IPLevelDQI
                {
                    IP = group.Key,
                    NoOfSitesAffected = previousCount,
                    TotalSites = group.ToList().Count(),
                    AverageConcurrence = concurrence.ToString("N2"),
                    WorstPerformingIndicator = indicator,
                });
            }

            dqiModel.IPLevel = iplevel;

            return dqiModel;
        }

        //public bool ProcessUpload(Stream uploadedFile, string v, Profile loggedinProfile, out string result)
        //{
        //    throw new NotImplementedException();
        //}

        public void PopulateTool(IPLevelDQI data, string directory, string fileName, string template)
        { 

            using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
            {
                var sheet = package.Workbook.Worksheets["IP Dashboard"];
                sheet.Cells["C2"].Value = data.IP;
                sheet.Cells["C4"].Value = data.WorstPerformingIndicator;
                sheet.Cells["C6"].Value = data.NoOfSitesAffected;
                sheet.Cells["G3"].Value = data.AverageConcurrence + "%";

                package.SaveAs(new FileInfo(directory + "/" + fileName));
            }
        }

        public string ProcessUpload(string filename, CommonUtil.Entities.Profile loggedinProfile)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filename)))
            {
                //get the report
                var report = new dqi_report_value();
                try
                {
                    var worksheet = package.Workbook.Worksheets["IP Dashboard"];
                    var wksheet = package.Workbook.Worksheets["Data Entry"];

                    var reported_data = new XElement("reported_data");
                    var indicators = new XElement("indicators");
                    indicators.Add(new XElement("Process Indicators", wksheet.Cells["A2"].Value.ToString()));
                    indicators.Add(new XElement("Process Indicators", wksheet.Cells["A4"].Value.ToString()));
                    indicators.Add(new XElement("Process Indicators", wksheet.Cells["A6"].Value.ToString()));
                    indicators.Add(new XElement("Process Indicators", wksheet.Cells["A8"].Value.ToString()));
                    indicators.Add(new XElement("Process Indicators", wksheet.Cells["A10"].Value.ToString()));
                    reported_data.Add(indicators);

                    var num = new XElement("numerators");
                    num.Add(new XElement("Week 1", wksheet.Cells["C2"].Value.ToString()));
                    num.Add(new XElement("Week 2", wksheet.Cells["D2"].Value.ToString()));
                    num.Add(new XElement("Week 3", wksheet.Cells["E2"].Value.ToString()));
                    num.Add(new XElement("Week 4", wksheet.Cells["F2"].Value.ToString()));
                    num.Add(new XElement("Week 5", wksheet.Cells["G2"].Value.ToString()));
                    num.Add(new XElement("Week 6", wksheet.Cells["H2"].Value.ToString()));
                    num.Add(new XElement("Week 7", wksheet.Cells["I2"].Value.ToString()));
                    num.Add(new XElement("Week 8", wksheet.Cells["J2"].Value.ToString()));
                    num.Add(new XElement("Week 9", wksheet.Cells["K2"].Value.ToString()));
                    num.Add(new XElement("Week 10", wksheet.Cells["L2"].Value.ToString()));
                    num.Add(new XElement("Week 11", wksheet.Cells["M2"].Value.ToString()));
                    num.Add(new XElement("Week 12", wksheet.Cells["N2"].Value.ToString()));
                    reported_data.Add(num);

                    var den = new XElement("denominators");
                    den.Add(new XElement("Week 1", wksheet.Cells["C3"].Value.ToString()));
                    den.Add(new XElement("Week 2", wksheet.Cells["D3"].Value.ToString()));
                    den.Add(new XElement("Week 3", wksheet.Cells["E3"].Value.ToString()));
                    den.Add(new XElement("Week 4", wksheet.Cells["F3"].Value.ToString()));
                    den.Add(new XElement("Week 5", wksheet.Cells["G3"].Value.ToString()));
                    den.Add(new XElement("Week 6", wksheet.Cells["H3"].Value.ToString()));
                    den.Add(new XElement("Week 7", wksheet.Cells["I3"].Value.ToString()));
                    den.Add(new XElement("Week 8", wksheet.Cells["J3"].Value.ToString()));
                    den.Add(new XElement("Week 9", wksheet.Cells["K3"].Value.ToString()));
                    den.Add(new XElement("Week 10", wksheet.Cells["L3"].Value.ToString()));
                    den.Add(new XElement("Week 11", wksheet.Cells["M3"].Value.ToString()));
                    den.Add(new XElement("Week 12", wksheet.Cells["N3"].Value.ToString()));
                    reported_data.Add(den);


                    report.ImplementingPartner = worksheet.Cells["C2"].ToString();
                    report.DqaIndicator = worksheet.Cells["C4"].ToString();
                    report.AffectedFacilityNumber = worksheet.Cells["C6"].ToString();
                    report.ImprovementApproach = worksheet.Cells["C8"].ToString();
                    report.DataCollectionMethod = worksheet.Cells["C10"].ToString();
                    report.Problem = worksheet.Cells["C13"].ToString();
                    report.ProblemResolved = worksheet.Cells["C14"].ToString() + worksheet.Cells["C15"].ToString();
                    report.WhyDoesProblemOccur = worksheet.Cells["D18"].ToString() + "-" + worksheet.Cells["D19"].ToString() + "-" + worksheet.Cells["D20"].ToString() + "-"+ worksheet.Cells["D21"].ToString() + worksheet.Cells["D22"].ToString();
                    report.ImprovementApproach_Analyze = worksheet.Cells["C23"].ToString();
                    report.Interventions = worksheet.Cells["D26"].ToString() + "-" + worksheet.Cells["D27"].ToString() + "-" + worksheet.Cells["D28"].ToString() + "-" + worksheet.Cells["D29"].ToString() + worksheet.Cells["D30"].ToString();
                    report.ImprovementApproach_Develop = worksheet.Cells["C31"].ToString();
                    report.ViableInterventions = worksheet.Cells["C34"].ToString();
                    report.ProcessTracking = worksheet.Cells["D35"].ToString() + "-" + worksheet.Cells["D36"].ToString() + "-" + worksheet.Cells["D37"].ToString() + "-" + worksheet.Cells["D38"].ToString() + worksheet.Cells["D39"].ToString();
                    report.MeasureIndicators = worksheet.Cells["C40"].ToString();
                    report.EvaluateIndicators = worksheet.Cells["C41"].ToString();
                    report.ReportPeriod = "Q1 FY18";
                    report.CreateDate = DateTime.Now;
                    report.CreatedBy = loggedinProfile.ContactEmailAddress;
                    report.Indicators = reported_data.ToString();

                    var report_valid = entities.dqi_report_value
                        .Where(e => e.ReportPeriod == report.ReportPeriod
                        && e.ImplementingPartner == report.ImplementingPartner);

                    foreach (var item in report_valid)
                    {
                        var thatUserRole = new CommonUtil.DAO.ProfileDAO().GetRoleByEmail(item.CreatedBy);
                        if (thatUserRole == loggedinProfile.RoleName)
                        {
                            return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>" + filename + " could not be processed. Report already exists in the database</td></tr>";
                        }
                    }
                    entities.dqi_report_value.Add(report);
                    entities.SaveChanges();          

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
                    Logger.LogInfo("QIEngine.ReadWorkbook", sb.ToString());
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>System error has occurred while processing the file " + filename + "</td></tr>";
                }
                catch (Exception ex)
                {
                    entities.dqi_report_value.Remove(report);
                    entities.SaveChanges();
                    Logger.LogError(ex);
                    return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>There are errors " + filename + "</td></tr>";
                }
            }
        }

    }
}
