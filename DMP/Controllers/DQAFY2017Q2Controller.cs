using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using DQA.DAL.Business;
using DQA.DAL.Model;
using ShieldPortal.ViewModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize(Roles = "shield_team,sys_admin,ip")]
    public class DQAFY2017Q2Controller : Controller
    {
        public ActionResult Index()
        {
            int ip_id = 0;
            if (User.IsInRole("ip"))
            {
                ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            }
            ViewBag.ip_id = ip_id;

            return View("Dashboard");
        }

        public ActionResult Analytics()
        {
            return View();
        }

        public ActionResult IpDQA()
        {
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                PopulateStates();
                return View("AllIPDQA");
            }

            else if (User.IsInRole("ip"))
            {
                var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
                ViewBag.ip_name = new Services.Utils().GetloggedInProfile().Organization.Name;

                PopulateStates();

                ViewBag.ip_id = ip_id;
                return View();
            }
            return View("~/Views/Shared/Denied.cshtml");
        }


        public ActionResult GetDQA(int id)
        {
            ViewBag.metadataId = id;
            return View();
        }

        public ActionResult IPDQAResult(int id)
        {
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                PopulateStates();
                PopulateStates();

                ViewBag.ip_id = id;
                return View("IpDQAAdmin");
            }
            return View("~/Views/Shared/Denied.cshtml");
        }

        public ActionResult UploadDQA()
        {
            var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            ViewBag.ip_id = ip_id;

            return View();
        }

        public ActionResult PendingFacilities()
        {
            return View();
        }

        public ActionResult DQAResult()
        {
            int ip_id = 0;
            if (User.IsInRole("ip"))
            {
                ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            }
            ViewBag.ip_id = ip_id;
            return View();
        }


        public void PopulateStates(object selectStatus = null)
        {
            var statusQuery = new BaseDAO<State, long>().RetrieveAll();
            ViewBag.states = new SelectList(statusQuery, "state_code", "state_name", selectStatus);
        }

        public ActionResult UploadPivotTable()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<UploadList> previousUploads = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = Utility.RetrievePivotTables(0);
            }
            else
            {
                previousUploads = Utility.RetrievePivotTables(profile.Organization.Id);
            }
            return View(previousUploads);
        }
         

        public ActionResult DownloadDQATool()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<PivotUpload> sites = new List<PivotUpload>();
             sites = Utility.RetrievePivotTablesForDQATool(profile.Organization.Id, "Q2(Jan-Mar)");
            return View(sites);
        }

        [HttpPost]
        public async Task<ActionResult> DownloadDQATool(List<PivotUpload> data)
        {
            string filename = "SHIELD_DQA_Q2.xlsm";
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q2/" + filename);

            var profile = new Services.Utils().GetloggedInProfile();

            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {

                //the radet period is hardcoded inside
                string file = await new BDQAQ2().GenerateDQA(data, stream, profile.Organization);

                return Json(file, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> downloadDQAResource(string fileType)
        {
            string filename = "";
            switch (fileType)
            { 
                case "DQAUserGuide":
                    filename = "USER GUIDE FOR THE DATA QUALITY ASSESSMENT TOOLv2May42017.pdf";
                    break;
                case "PivotTable":
                    filename = "DATIM PIVOT TABLE SAMPLE.xlsx";
                    break;
            }
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q2/" + filename);

            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
        }

        /*
        public ActionResult UploadRadet()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<RadetUploadReport> previousUploads = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = new RadetUploadReportDAO().RetrieveRadetUpload(0, 2017, "Q2(Jan-Mar)");
                ViewBag.showdelete = true;
            }
            else
            {
                ViewBag.showdelete = false;
                previousUploads = new RadetUploadReportDAO().RetrieveRadetUpload(profile.Organization.Id, 2017, "Q2(Jan-Mar)");
            }
            List<RadetReportModel> list = new List<ViewModel.RadetReportModel>();
            if (previousUploads != null)
            {
                list = (from entry in previousUploads
                        select new RadetReportModel
                        {
                            Facility = entry.Facility,
                            IP = entry.IP.ShortName,
                            dqa_quarter = entry.dqa_quarter,
                            dqa_year = entry.dqa_year,
                            UploadedBy = entry.UploadedBy.FullName,
                            DateUploaded = entry.DateUploaded,
                            Id = entry.Id, 
                        }).ToList();
            }

            return View(list);
        }
        */

    }
}