using System;
using System.Web.Mvc;
using BWReport.DAL.DAO;
using BWReport.DAL.Services;
using System.Net;
using System.IO;
using DMP.ViewModel.BWR;
using CommonUtil.DAO;
using System.Linq;
using System.Web.Helpers;
using BWReport.DAL.Entities;
using System.Collections.Generic;

namespace DMP.Controllers
{
    [Authorize]
    public class BiWeeklyReportController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            PerformanceDataDao pDao = new PerformanceDataDao();
             
            var GroupedLGAAcheivement = pDao.GenerateLGALevelAchievementPerTarget(2017);

            var positivityRates = pDao.ComputePositivityRateByFacilityType(2017);

            ReportViewModel reportList = new ReportViewModel
            {
                LGAReports = GroupedLGAAcheivement,
                CommunityPositivty = positivityRates.ToList().FindAll(x => x.FacilityType == CommonUtil.Enums.OrganizationType.CommunityBasedOrganization),
                FacilityPositivty = positivityRates.ToList().FindAll(x => x.FacilityType == CommonUtil.Enums.OrganizationType.HealthFacilty),
                ImplementingPartner = new OrganizationDAO().RetrieveAll().Select(x => x.ShortName).ToList()
            };

            return View(reportList);
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

            int startColumnIndex = new ReportViewModel().IndexPeriods[reportingPeriod];

            try
            {
                bool result = new ReportLoader().ExtractReport(reportingPeriod, Year, startColumnIndex, ImplementingPartner, fileContent, loggedInUser);
                return Json("Upload succesful", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
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
            if (saved)
                return Json("Upload Succesful");
            else
                return new HttpStatusCodeResult(HttpStatusCode.Conflict, wrongEntries);
        }
    }
}