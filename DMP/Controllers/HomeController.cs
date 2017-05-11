using CommonUtil.DAO;
using CommonUtil.Utilities;
using DAL.DAO;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        DMPDAO dmpDAO = null;
        DMPDocumentDAO dmpDocDAO = null;
        OrganizationDAO orgDAO = null;
        ProjectDetailsDAO projDAO = null;

        public HomeController()
        {
            dmpDAO = new DMPDAO();
            dmpDocDAO = new DMPDocumentDAO();
            orgDAO = new OrganizationDAO();
            projDAO = new ProjectDetailsDAO();

        }
        
        public ActionResult index()
        {
            return View();
        }

        public ActionResult submenu(string id)
        {
            if (id.Contains("routine_reporting"))
            {
                ViewBag.breadcrumb = "Routine Reporting";
            }
            if (id.Contains("dmp"))
            {
                ViewBag.breadcrumb = "Data Management";
            }
            if (id.Contains("hiv_aid"))
            {
                ViewBag.breadcrumb = "Hiv-Aids Monitoring and Evaluation";
            } 
            if (id.Contains("outcome"))
            {
                ViewBag.breadcrumb = "Outcome Evaluation";
            }
            if (id.Contains("admin"))
            {
                ViewBag.breadcrumb = "Admin";
            }
             
            ViewBag.Container = id;
            return View();
        }
 
        public async Task<ActionResult> downloadDMPResource(string fileType)
        {
            string filename = "";
            if (fileType.Contains("staffstructure"))
                filename = "Implementing Partner ME Staff Structure_modified.xlsx";

            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/" + filename);

            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
        }

        public async Task<ActionResult> downloadDQAResource(string fileType)
        {
            string filename = "Data Quality Assessment Tool.Q1R.xlsm";
            switch (fileType)
            {
                case "DQATemplate":
                    filename = "SHIELD_DQA_v2_20170321.xlsm";
                    break;
                case "DQASummaryReport":
                    filename = "DQA Summary Report.Q1R.pdf";
                    break;
                case "DQAUserGuide":
                    filename = "GUIDE TO USING THE DATA QUALITY ASSESSMENT (DQA) TOOL.Q1R.pdf";
                    break;
                case "RandomNumber":
                    filename = "Random Number Table.pdf";
                    break;
                case "PivotTable":
                    filename = "DATIM PIVOT TABLE SAMPLE.xlsx";
                    break;
            }
            //Report/Template\SHIELD_DQA_v2_20170321.xlsm
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/" + filename);

            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
        }
         

        [Authorize(Users = "emadubuko")]
        [HttpGet]
        public ActionResult Tracker(Guid documnentId)
        {
            var doc = dmpDocDAO.Retrieve(documnentId);

            if (doc.Document.MonitoringAndEvaluationSystems == null || doc.Document.QualityAssurance == null || doc.Document.DataProcesses.Reports == null)
            {
                return new HttpStatusCodeResult(400, "DMP document is not yet completed");
            }

            List<GanttChartData> chartData = new List<GanttChartData>();
            List<ChartValues> values = new List<ChartValues>();

            try
            {
                var trainings = doc.Document.MonitoringAndEvaluationSystems.People.Trainings;
                var dataCollection = doc.Document.DataProcesses.DataCollection;
                var dataVerification = doc.Document.QualityAssurance.DataVerification;
                var reports = doc.Document.DataProcesses.Reports.ReportData;

                var trainingGntChart = new List<GanttChartData>();

                foreach (var tr in trainings)
                {
                    DateTime siteStartdt = new DateTime();
                    DateTime siteEndDate = new DateTime();

                    var aTr = new GanttChartData();
                    aTr.desc = tr.NameOfTraining;
                    aTr.values = new List<ChartValues>();

                    DateTime.TryParse(tr.SiteStartDate, out siteStartdt);
                    DateTime.TryParse(tr.SiteEndDate, out siteEndDate);

                    if (siteStartdt > DateTime.MinValue && siteEndDate > DateTime.MinValue)
                    {
                        aTr.values.Add(
                            new ChartValues
                            {
                                customClass = Labels.trainingLabelClass,
                                from = siteStartdt,
                                to = siteEndDate,
                                label = "Site training", //Labels.trainingLabelName,
                                dataObj = "Site training date",
                            });
                    }

                    DateTime regionStartdt = new DateTime();
                    DateTime regionndDate = new DateTime();

                    DateTime.TryParse(tr.RegionStartDate, out regionStartdt);
                    DateTime.TryParse(tr.RegionEndDate, out regionndDate);

                    if (regionStartdt > DateTime.MinValue && regionndDate > DateTime.MinValue)
                    {
                        aTr.values.Add(
                            new ChartValues
                            {
                                customClass = Labels.trainingLabelClass,
                                from = regionStartdt,
                                to = regionndDate,
                                label = "Region training", //Labels.trainingLabelName,
                                dataObj = "Region training date",
                            });
                    }

                    DateTime hqStartdt = new DateTime();
                    DateTime hqEndDate = new DateTime();

                    DateTime.TryParse(tr.HQStartDate, out hqStartdt);
                    DateTime.TryParse(tr.HQEndDate, out hqEndDate);

                    if (hqStartdt > DateTime.MinValue && hqEndDate > DateTime.MinValue)
                    {
                        aTr.values.Add(
                            new ChartValues
                            {
                                customClass = Labels.trainingLabelClass,
                                from = hqStartdt,
                                to = hqEndDate,
                                label = "HQ training", //Labels.trainingLabelName,
                                dataObj = "HQ training date",
                            });
                    }

                    trainingGntChart.Add(aTr);
                }

                trainingGntChart.FirstOrDefault().name = "Training";
                chartData.AddRange(trainingGntChart);

                values = new List<ChartValues>();
                foreach (var rpt in reports)
                {
                    rpt.TimelinesForReporting.ForEach(z =>
                    {
                        if (z > DateTime.MinValue)
                        {
                            values.Add(new ChartValues
                            {
                                customClass = Labels.reportLabelClass,
                                from = z,
                                to = z.AddDays(rpt.DurationOfReporting),
                                label = rpt.ReportsType, //Labels.reportLabelName,
                                dataObj = rpt.ReportsType,
                            });
                        }
                    });
                }
                chartData.Add(
                        new GanttChartData
                        {
                            name = "Report",
                            values = values
                        });

                values = new List<ChartValues>();
                foreach (var dv in dataVerification)
                {
                    dv.TimelinesForDataVerification.ForEach(z =>
                    {
                        if (z > DateTime.MinValue)
                        {
                            values.Add(new ChartValues
                            {
                                customClass = Labels.dataVerificationLabelClass,
                                from = z,
                                to = z.AddDays(dv.DurationOfDataVerificaion),
                                label = dv.DataVerificationApproach,  //Labels.dataVerificationLabelName,
                                dataObj = dv.TypesOfDataVerification + "\n " + dv.DataVerificationApproach,
                            });
                        }
                    });
                }
                chartData.Add(
                        new GanttChartData
                        {
                            name = "Data validation",
                            values = values
                        });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }

            TrackerViewModel vM = new TrackerViewModel
            {
                data = chartData,
                DocumentTitle = doc.DocumentTitle
            };

            return View(vM);
        }
    }

    public enum downloadType
    {
        DQATemplate = 1,
        DQASummaryReport = 2,
        DQAUserGuide = 3,
        RandomNumber = 4
    }

    
}
