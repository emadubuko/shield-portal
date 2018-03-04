using CommonUtil.DAO;
using CommonUtil.Entities;
using MPM.DAL;
using MPM.DAL.DAO;
using MPM.DAL.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class MPMController : Controller
    {
        // GET: MPM
        public ActionResult Index()
        {
            Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
            var vm = new UploadViewModel();
            vm.ImplementingPartner = loggedinProfile.RoleName == "ip" ? new List<string> { loggedinProfile.Organization.ShortName } : new OrganizationDAO().RetrieveAll().Select(x => x.ShortName).ToList();


            vm.IPReports = new Dictionary<string, List<bool>>();

            var reports = new MPMDAO().GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "").GroupBy(x => x.IPName);
            foreach (var r in reports)
            {
                List<bool> uploaded = new List<bool>(12);
                var ipEntries = r.ToList().Select(x => x.ReportPeriod).ToList();

                foreach (var index in IndexPeriods.Keys)
                {
                    if (ipEntries.Any(x => x.Contains(index)))
                        uploaded.Add(true);
                    else
                        uploaded.Add(false);
                }

                vm.IPReports.Add(r.Key, uploaded);
            }
            return View(vm);
        }

        public ActionResult Upload()
        {
           
            return View();
        }

        public JsonResult ProcessFile()
        {
            Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
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
            catch(Exception ex)
            {
                return Json("<span style='color: red;'>" + ex.Message + "</span>");
            }
        }
         

        public async Task<ActionResult> DownloadTemplate()
        {
            Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
            string file = new TemplateProcessor().PopulateTemplate(loggedinProfile);

            using (var stream = System.IO.File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "CDC MPM Tool_" + loggedinProfile.Organization.ShortName + ".xlsm");
            } 
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