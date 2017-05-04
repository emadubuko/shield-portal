using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using DQA.DAL.Model;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize(Roles = "shield_team,sys_admin,ip")]
    public class DQAController : Controller
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
        

        //[HttpPost]
        //public ActionResult DownloadDQADimensions()
        //{            
        //    string fileName = "DQADimensions.csv";
        //    string fullFilename = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Downloads/" + fileName);

        //    using (FileStream fs = new FileStream(fullFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        //    {

        //    }

        //    return Json(fileName);
        //}
    }
}
