using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using MPM.DAL;
using MPM.DAL.DAO;
using MPM.DAL.Processor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class MPMController : Controller
    {
        Profile loggedinProfile;

        public MPMController()
        {
            loggedinProfile = new Services.Utils().GetloggedInProfile();
        }

        // GET: MPM
        public ActionResult Index()
        {
            var vm = new UploadViewModel();
            vm.IPReports = new Dictionary<string, List<bool>>();
            var mpmDAO = new MPMDAO();

            var reports = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", null)
                .GroupBy(x => x.IPName);
            foreach (var iplevel in reports)
            {
                vm.ImplementingPartner = iplevel.Key;
                foreach (var state in iplevel.GroupBy(x => x.ReportingLevelValue))
                {
                    var entries = state.ToList().Select(x => x.ReportPeriod).ToList();
                    List<bool> uploaded = new List<bool>(12);
                    foreach (var index in IndexPeriods.Keys)
                    {
                        if (entries.Any(x => x.Contains(index)))
                            uploaded.Add(true);
                        else
                            uploaded.Add(false);
                    }
                    vm.IPReports.Add(state.Key + "|" + iplevel.Key, uploaded);
                }
            }
            ViewBag.reportPeriod = mpmDAO.GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);

            return View(vm);
        }

        public JsonResult ReportDetails(string reportPeriod)
        {
            var mpmDAO = new MPMDAO();
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            if (string.IsNullOrEmpty(reportPeriod))
                reportPeriod = mpmDAO.GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);

            DataTable reports = null;

            try
            {
                reports = mpmDAO.GetUploadReport(reportPeriod);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }


            List<dynamic> reportView = new List<dynamic>();

            foreach (DataRow dr in reports.Rows)
            {
                if (!string.IsNullOrEmpty(ip) && dr.Field<string>("IP") != ip)
                    continue;
                reportView.Add(new
                {
                    IP = dr.Field<string>("IP"),
                    State = dr.Field<string>("State"),
                    LGA = dr.Field<string>("LGA"),
                    Facility = dr.Field<string>("Facility"),

                    PMTCT = dr.Field<string>("PMTCT"),
                    PMTCT_EID = dr.Field<string>("PMTCT_EID"),
                    ART = dr.Field<string>("ART"),
                    HTS = dr.Field<string>("HTS"),
                    HTS_PITC = dr.Field<string>("HTS_PITC"),
                    LinkageToTx = dr.Field<string>("Linkage_To_Treatment"),
                    PMTCT_Viral_Load = dr.Field<string>("PMTCT_Viral_Load"),
                    TB_hiv_tx = dr.Field<string>("TB_HIV_Treatment"),
                    TB_Presumptive = dr.Field<string>("TB_Presumptive"),
                    TB_Presumptive_Diagnosed = dr.Field<string>("TB_Presumptive_Diagnosed"),
                    TPT_Completed = dr.Field<string>("TPT_Completed"),
                    TPT_Eligible = dr.Field<string>("TPT_Eligible"),
                });
            }
            //ViewBag.CompletionReport = reportView;
            return Json(reportView);
        }

        public ActionResult Upload()
        {
            return View();
        }

        public async Task<ActionResult> NDR_Statictics()
        {
            var data = await new NDR_StatisticsDAO().RetrieveAll();
            string reportPeriod = new MPMDAO().GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;

                data = data.Where(x => x.Facility.Organization.Id == profile.Organization.Id).ToList();
            }

            data = data//.Where(x => x.ReportPeriod == reportPeriod)
                .OrderByDescending(o => o.Facility.GranularSite).ToList();

            ViewBag.reportPeriod = reportPeriod;
            return View(data);
        }

        public JsonResult ProcessFile()
        {
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/MPM/" + loggedinProfile.Organization.ShortName) + "\\";
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            if (Request.Files.Count == 0)
                return Json("<span style='color: red;'> no file uploaded</span>");

            var filePath = directory + DateTime.Now.ToString("dd MMM yyyy") + "_" + Request.Files[0].FileName;
            Request.Files[0].SaveAs(filePath);

            try
            {
                new TemplateProcessor().ReadFile(Request.Files[0].InputStream, loggedinProfile);
                return Json("Uploaded succesfully");
            }
            catch (Exception ex)
            {
                return Json("<span style='color: red;'>" + ex.Message + "</span>");
            }
        }

        public ActionResult Download()
        {
            var facilities = new HealthFacilityDAO().RetrievebyIP(loggedinProfile.Organization.Id);
            List<State> states = new List<State>();
            if (facilities != null)
                states.AddRange(facilities.Select(x => x.LGA.State).Distinct());

            return View(states);
        }

        [HttpPost]
        public JsonResult DownloadTemplate(string state)
        {
            string file = new TemplateProcessor().PopulateTemplate(loggedinProfile, state);
            return Json(file, JsonRequestBehavior.AllowGet);
            //using (var stream = System.IO.File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //{
            //    byte[] fileBytes = new byte[stream.Length];
            //    await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
            //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "CDC MPM Tool_" + loggedinProfile.Organization.ShortName + "_" + state + ".xlsm");
            //} 
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public Dictionary<string, int> IndexPeriods
        {
            get
            {
                return new Dictionary<string, int>
                {
                    { "Jan",1 },
                    { "Feb",2 },
                    { "Mar",3 },
                    { "Apr",4 },
                    { "May",5 },
                    { "Jun",6 },
                    { "Jul",7 },
                    { "Aug",8 },
                    { "Sep",9 },
                    { "Oct",10 },
                    { "Nov",11},
                    { "Dec",12 }
                };
            }
        }
    }
}