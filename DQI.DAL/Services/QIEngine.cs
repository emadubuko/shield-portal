using CommonUtil.DAO;
using CommonUtil.Entities;
using DQI.DAL.DAO;
using DQI.DAL.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq; 
using System.Text;
using CommonUtil.Utilities;
using System.Xml.Linq;
using DQA.DAL.Data;
using System.Data.Entity.Validation;

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

        public List<IPLevelDQI> RetrieveUpload(Profile loggedinProfile, string report_period)
        {
            string ip = loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.ShortName : "";
            if (!string.IsNullOrEmpty(ip))
            {
                return (from item in entities.dqi_report_value.Where(e => e.ReportPeriod == report_period && e.ImplementingPartner == ip)
                        select new IPLevelDQI
                        {
                            Id = item.Id,
                            IP = item.ImplementingPartner,
                            WorstPerformingIndicator = item.DqaIndicator,
                            AffectedSites = item.AffectedFacilityNumber,
                            LastUpdatedDate = item.LastUpdatedDate,
                            UploadedBy = item.uploadedBy
                        }).ToList();
            }
            else
            {
                return (from item in entities.dqi_report_value.Where(e => e.ReportPeriod == report_period)
                        select new IPLevelDQI
                        {
                            Id = item.Id,
                            IP = item.ImplementingPartner,
                            WorstPerformingIndicator = item.DqaIndicator,
                            AffectedSites = item.AffectedFacilityNumber,
                            LastUpdatedDate = item.LastUpdatedDate,
                            UploadedBy = item.uploadedBy
                        }).ToList();
            }
        }

        public dqi_report_value RetrieveUpload(int id)
        {
            return entities.dqi_report_value.Where(e => e.Id == id).FirstOrDefault(); 
        }

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

        public string ProcessUpload(string filename, Profile loggedinProfile, string ReportPeriod)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filename)))
            {
                //get the report
                var report = new dqi_report_value();
                try
                {
                    var datasheet = package.Workbook.Worksheets["IP Dashboard"];
                    
                    report.ImplementingPartner = datasheet.Cells["C2"].Value.ToString();
                    report.DqaIndicator = datasheet.Cells["C4"].Value.ToString();
                    report.AffectedFacilityNumber = datasheet.Cells["C6"].Value.ToString();

                    if (datasheet.Cells["C8"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>What quality improvement approach will you use?</b></td></tr>";
                    }
                    else
                    {
                        report.ImprovementApproach = datasheet.Cells["C8"].Value.ToString();
                    }
                    if (datasheet.Cells["C10"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>What data collection method are you using for this?</b></td></tr>";
                    }
                    report.DataCollectionMethod = datasheet.Cells["C10"].Value.ToString();

                    if (datasheet.Cells["C13"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>What is the problem?</b></td></tr>";
                    }
                    report.Problem = datasheet.Cells["C13"].Value.ToString();

                    if (datasheet.Cells["C14"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>How do you know this is a problem?</b></td></tr>";
                    }
                    string how_you_know_this_is_the_problem = datasheet.Cells["C14"].Value.ToString();

                    if (datasheet.Cells["C15"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>How will you know when the problem is resolved?</b></td></tr>";
                    }
                    report.ProblemResolved = datasheet.Cells["C15"].Value.ToString();

                    var whyProblemOccurXML = new XElement("Why_problem_occur",
                                                            new XElement("i", datasheet.Cells["D18"].Value),
                                                            new XElement("ii", datasheet.Cells["D19"].Value),
                                                            new XElement("iii", datasheet.Cells["D20"].Value),
                                                            new XElement("iv", datasheet.Cells["D21"].Value),
                                                            new XElement("v", datasheet.Cells["D22"].Value));
                    report.WhyDoesProblemOccur = whyProblemOccurXML.Value.ToString();

                    if (datasheet.Cells["C23"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>Which Quality Improvement approach did you use to analyze the problem?</b></td></tr>";
                    }
                    report.ImprovementApproach_Analyze = datasheet.Cells["C23"].Value.ToString();

                    var interventionXML = new XElement("interventions",
                                                new XElement("i", datasheet.Cells["D26"].Value),
                                                new XElement("ii", datasheet.Cells["D27"].Value),
                                                new XElement("iii", datasheet.Cells["D28"].Value),
                                                new XElement("iv", datasheet.Cells["D29"].Value),
                                                new XElement("v", datasheet.Cells["D30"].Value));
                    report.Interventions = interventionXML.ToString();

                    if (datasheet.Cells["C31"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>Which Quality Improvement approach did you apply in developing these interventions?</b></td></tr>";
                    }
                    report.ImprovementApproach_Develop = datasheet.Cells["C31"].Value.ToString();

                     
                    var processTrackingXML = new XElement("process_tracking",
                                                        new XElement("i", datasheet.Cells["D35"].Value),
                                                        new XElement("ii", datasheet.Cells["D36"].Value),
                                                        new XElement("iii", datasheet.Cells["D37"].Value),
                                                        new XElement("iv", datasheet.Cells["D38"].Value),
                                                        new XElement("v", datasheet.Cells["D39"].Value));
                    //report.ViableInterventions = processTrackingXML.ToString(); // datasheet.Cells["C34"].ToString();
                    report.ProcessTracking = processTrackingXML.ToString();

                    if (datasheet.Cells["C40"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>How often will you measure these chosen indicators?</b></td></tr>";
                    }
                    report.MeasureIndicators = datasheet.Cells["C40"].Value.ToString();

                    if (datasheet.Cells["C41"].Value == null)
                    {
                        return "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td> please provide response for the question <b>How often will you evaluate the indicators?</b></td></tr>";
                    }
                    report.EvaluateIndicators = datasheet.Cells["C41"].Value.ToString();

                    report.ReportPeriod = ReportPeriod;
                    report.LastUpdatedDate = DateTime.Now;
                    report.uploadedBy = loggedinProfile.Username;

                    var entrysheet = package.Workbook.Worksheets["Data Entry"];

                    List<ProcessTable> processTable = ReadExcelTable(entrysheet);
                     
                    report.Indicators = XMLUtil.ConvertToXml(processTable);
                     
                    var report_valid = entities.dqi_report_value
                        .Where(e => e.ReportPeriod == report.ReportPeriod
                        && e.ImplementingPartner == report.ImplementingPartner);

                    foreach (var item in report_valid)
                    {
                        entities.dqi_report_value.Remove(item);                      
                    }
                    entities.dqi_report_value.Add(report);
                    entities.SaveChanges();          

                    return "00|<i class='icon-check icon-larger green-color'></i>File was processed successfully";
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


        List<ProcessTable> ReadExcelTable(ExcelWorksheet entrysheet)
        {
            List<ProcessTable> processTable = new List<ProcessTable>();
            for (int row = 2; row <= 10;)
            {
                var cell = entrysheet.Cells["A" + row];

                if (cell != null && cell.Value != null && cell.Value.ToString() != "0")
                {
                    processTable.Add(new ProcessTable
                    {
                        Name = ExcelHelper.ReadCellText(entrysheet, row, 1),
                        Numerator_Definition = ExcelHelper.ReadCellText(entrysheet, row, 2),
                        Denominator_Definition = ExcelHelper.ReadCellText(entrysheet, row + 1, 2),
                        Week_1 = new WeekData
                        {
                            Numerator = entrysheet.Cells["C" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["C" + (row + 1)].Value.ToInt()
                        },
                        Week_2 = new WeekData
                        {
                            Numerator = entrysheet.Cells["D" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["D" + (row + 1)].Value.ToInt()
                        },
                        Week_3 = new WeekData
                        {
                            Numerator = entrysheet.Cells["E" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["E" + (row + 1)].Value.ToInt()
                        },
                        Week_4 = new WeekData
                        {
                            Numerator = entrysheet.Cells["F" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["F" + (row + 1)].Value.ToInt()
                        },
                        Week_5 = new WeekData
                        {
                            Numerator = entrysheet.Cells["G" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["G" + (row + 1)].Value.ToInt()
                        },
                        Week_6 = new WeekData
                        {
                            Numerator = entrysheet.Cells["H" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["H" + (row + 1)].Value.ToInt()
                        },
                        Week_7 = new WeekData
                        {
                            Numerator = entrysheet.Cells["I" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["I" + (row + 1)].Value.ToInt()
                        },
                        Week_8 = new WeekData
                        {
                            Numerator = entrysheet.Cells["J" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["J" + (row + 1)].Value.ToInt()
                        },
                        Week_9 = new WeekData
                        {
                            Numerator = entrysheet.Cells["K" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["K" + (row + 1)].Value.ToInt()
                        },
                        Week_10 = new WeekData
                        {
                            Numerator = entrysheet.Cells["L" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["L" + (row + 1)].Value.ToInt()
                        },
                        Week_11 = new WeekData
                        {
                            Numerator = entrysheet.Cells["M" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["M" + (row + 1)].Value.ToInt()
                        },
                        Week_12 = new WeekData
                        {
                            Numerator = entrysheet.Cells["N" + row].Value.ToInt(),
                            Denominator = entrysheet.Cells["N" + (row + 1)].Value.ToInt()
                        }
                    });
                }
                row = row + 2;
            }
            return processTable;
        }

    }

    
}


//var reported_data = new XElement("reported_data");
//var indicators = new XElement("indicators");
//indicators.Add(new XElement("process_indicator", entrysheet.Cells["A2"].Value));
//                    indicators.Add(new XElement("process_indicator", entrysheet.Cells["A4"].Value));
//                    indicators.Add(new XElement("process_indicator", entrysheet.Cells["A6"].Value));
//                    indicators.Add(new XElement("process_indicator", entrysheet.Cells["A8"].Value));
//                    indicators.Add(new XElement("process_indicator", entrysheet.Cells["A10"].Value));
//                    reported_data.Add(indicators);

//                    var num = new XElement("numerators");
//num.Add(new XElement("Week_1", entrysheet.Cells["C2"].Value));
//                    num.Add(new XElement("Week_2", entrysheet.Cells["D2"].Value));
//                    num.Add(new XElement("Week_3", entrysheet.Cells["E2"].Value));
//                    num.Add(new XElement("Week_4", entrysheet.Cells["F2"].Value));
//                    num.Add(new XElement("Week_5", entrysheet.Cells["G2"].Value));
//                    num.Add(new XElement("Week_6", entrysheet.Cells["H2"].Value));
//                    num.Add(new XElement("Week_7", entrysheet.Cells["I2"].Value));
//                    num.Add(new XElement("Week_8", entrysheet.Cells["J2"].Value));
//                    num.Add(new XElement("Week_9", entrysheet.Cells["K2"].Value));
//                    num.Add(new XElement("Week_10", entrysheet.Cells["L2"].Value));
//                    num.Add(new XElement("Week_11", entrysheet.Cells["M2"].Value));
//                    num.Add(new XElement("Week_12", entrysheet.Cells["N2"].Value));
//                    reported_data.Add(num);

//                    var den = new XElement("denominators");
//den.Add(new XElement("Week_1", entrysheet.Cells["C3"].Value));
//                    den.Add(new XElement("Week_2", entrysheet.Cells["D3"].Value));
//                    den.Add(new XElement("Week_3", entrysheet.Cells["E3"].Value));
//                    den.Add(new XElement("Week_4", entrysheet.Cells["F3"].Value));
//                    den.Add(new XElement("Week_5", entrysheet.Cells["G3"].Value));
//                    den.Add(new XElement("Week_6", entrysheet.Cells["H3"].Value));
//                    den.Add(new XElement("Week_7", entrysheet.Cells["I3"].Value));
//                    den.Add(new XElement("Week_8", entrysheet.Cells["J3"].Value));
//                    den.Add(new XElement("Week_9", entrysheet.Cells["K3"].Value));
//                    den.Add(new XElement("Week_10", entrysheet.Cells["L3"].Value));
//                    den.Add(new XElement("Week_11", entrysheet.Cells["M3"].Value));
//                    den.Add(new XElement("Week_12", entrysheet.Cells["N3"].Value));
//                    reported_data.Add(den);
                    
//                    report.Indicators = reported_data.ToString();