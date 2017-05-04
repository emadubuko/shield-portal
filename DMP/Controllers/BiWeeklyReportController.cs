using System;
using System.Web.Mvc;
using BWReport.DAL.DAO;
using BWReport.DAL.Services;
using System.Net;
using System.IO;
using ShieldPortal.ViewModel.BWR;
using CommonUtil.DAO;
using System.Linq;
using BWReport.DAL.Entities;
using System.Collections.Generic;
using CommonUtil.Utilities;
using System.Runtime.InteropServices;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class BiWeeklyReportController : Controller
    {
        static Dictionary<string, int> IndexPeriods;

        public BiWeeklyReportController()
        {
            if (IndexPeriods == null)
            {
                IndexPeriods = ExcelHelper.GenerateIndexedPeriods();
            }
        }

        public ActionResult Index(int fYear = 2017)
        {
            ViewBag.Title = "Home Page";
            PerformanceDataDao pDao = new PerformanceDataDao();

            var loggedinProfile = new Services.Utils().GetloggedInProfile();

            var GroupedLGAAcheivement = pDao.GenerateLGALevelAchievementPerTarget(fYear, loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);

            var positivityRates = pDao.ComputePositivityRateByFacilityType(fYear, loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);

            ReportViewModel reportList = new ReportViewModel
            {
                SelectedYear = fYear.ToString(),
                LGAReports = GroupedLGAAcheivement,
                CommunityPositivty = positivityRates.ToList().FindAll(x => x.FacilityType == CommonUtil.Enums.OrganizationType.CommunityBasedOrganization),
                FacilityPositivty = positivityRates.ToList().FindAll(x => x.FacilityType == CommonUtil.Enums.OrganizationType.HealthFacilty),
                ImplementingPartner = loggedinProfile.RoleName == "ip" ? new List<string> { loggedinProfile.Organization.ShortName } : new OrganizationDAO().RetrieveAll().Select(x => x.ShortName).ToList(),
            };
            ViewBag.IndexPeriods = IndexPeriods.Keys.ToList();

            return View(reportList);
        }

        public ActionResult Dashboard()
        {
            Dictionary<string, string> iframe = null;
            var loggedinProfile = new Services.Utils().GetloggedInProfile();

            iframe = Utilities.RetrieveDashboard("shield_team", "national");

            //if (loggedinProfile.RoleName == "ip")
            //{
            //    iframe = Utilities.RetrieveDashboard(loggedinProfile.RoleName, loggedinProfile.Organization.ShortName);
            //}
            //else
            //{
            //    iframe = Utilities.RetrieveDashboard("shield_team", "national");
            //}

            foreach (var item in iframe)
            {
                ViewData[item.Key] = item.Value;
            }
            return View();
        }

        public ActionResult UploadNewReport()
        {
            ViewBag.IndexPeriods = IndexPeriods.Keys.ToList();
            var vm = new BiWeeklyReportUploadViewModel();
            var loggedinProfile = new Services.Utils().GetloggedInProfile();
            vm.IndexPeriods = IndexPeriods;

            vm.ImplementingPartner = loggedinProfile.RoleName == "ip" ? 
                new List<string> { loggedinProfile.Organization.ShortName } : new OrganizationDAO().RetrieveAll().Select(x => x.ShortName).ToList();
           
            return View(vm);
        }

        [HttpPost]
        public ActionResult Upload(string reportingPeriod, int Year, string ImplementingPartner)
        {
            var files = Request.Files;
            if (files == null || files.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "no files uploaded");
            }
            string fileName = files[0].FileName;
            string fullfilename = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/_" + fileName);

            Stream fileContent = files[0].InputStream;
            string loggedInUser = Services.Utils.LoggedinProfileName;

            int startColumnIndex = IndexPeriods[reportingPeriod];

            try
            {
                bool result = new ReportLoader().ExtractReport(reportingPeriod, Year, startColumnIndex, ImplementingPartner, fileContent, loggedInUser, fileName);
                return Json("00|Upload succesful", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("06|"+ex.Message, JsonRequestBehavior.AllowGet);
                //return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public ActionResult BiWeeklyReportUploads(int fYear = 2017)
        {
            ViewBag.IndexPeriods = IndexPeriods.Keys.ToList();
            PerformanceDataDao pDao = new PerformanceDataDao();
            var loggedinProfile = new Services.Utils().GetloggedInProfile();

            var reports = pDao.GenerateIPUploadReports(fYear, loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0).GroupBy(x => x.IPName);

            List<BiWeeklyReportUploadViewModel> vMReport = new List<BiWeeklyReportUploadViewModel>();
            var vm = new BiWeeklyReportUploadViewModel();
            vm.IPReports = new Dictionary<string, List<bool>>();

            foreach (var r in reports)
            {
                List<bool> uploaded = new List<bool>(IndexPeriods.Count);
                var ipEntries = r.ToList().Select(x => x.ReportPeriod).ToList();

                foreach (var index in IndexPeriods.Keys)
                {
                    if (ipEntries.FirstOrDefault(x => x == index) != null)
                        uploaded.Add(true);
                    else
                        uploaded.Add(false);
                }

                vm.IPReports.Add(r.Key, uploaded);
            }

            vm.IndexPeriods = IndexPeriods;

            vm.ImplementingPartner = loggedinProfile.RoleName == "ip" ? new List<string> { loggedinProfile.Organization.ShortName } : new OrganizationDAO().RetrieveAll().Select(x => x.ShortName).ToList();
            vm.SelectedYear = fYear.ToString();
            return View(vm);
        }


        public ActionResult BiWeeklyReportDownload(string IP, int fYear)
        {
            string IpSample = IP + "_sample.xlsx";
            string existingTemplate = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/" + IpSample);

            string fileName = IP + ".xlsx";
            string newFile = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/" + fileName);

            new ReportLoader().GenerateExcel(newFile, existingTemplate, IP, fYear);

            byte[] fileBytes = System.IO.File.ReadAllBytes(newFile);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }



        public ActionResult ManageYearlyPerformanceTarget()
        {
            YearlyPerfomanceTargetViewModel vM = new YearlyPerfomanceTargetViewModel
            {
                Facilities = new HealthFacilityDAO().RetrieveAll(),
                Targets = new YearlyPerformanceTargetDAO().RetrieveAll(),
            };
            return View(vM);
        }

        [HttpPost]
        public ActionResult CreateNewPerformanceTarget(YearlyPerformanceTarget Target)
        {
            YearlyPerformanceTargetDAO dao = new YearlyPerformanceTargetDAO();
            if (dao.GetTargetByYear(Target.FiscalYear, Target.HealthFaciltyId) == null)
            {
                Target.HealthFacilty = new HealthFacilityDAO().Retrieve(Target.HealthFaciltyId);
                dao.Save(Target);
                dao.CommitChanges();

                return Json("Saved Succesfully", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Conflict, "Target is alreadt set for the facility for the selected year");
            }
        }

        public ActionResult UploadNewPerformanceTarget(int fiscalyear)
        {
            string wrongEntries = "";
            YearlyPerformanceTargetDAO dao = new YearlyPerformanceTargetDAO();
            var files = Request.Files;
            if (files == null || files.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "no files uploaded");
            }

            Stream fileContent = files[0].InputStream;
            bool saved = dao.SaveBatchFromCSV(fileContent, fiscalyear, out wrongEntries);
            if (string.IsNullOrEmpty(wrongEntries))
                return Json("Upload Succesful");
            else
            {
                var data = new { error = wrongEntries };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            // return new HttpStatusCodeResult(HttpStatusCode.BadRequest, wrongEntries);
        }
    }
}