using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class DQAController : Controller
    {
        public ActionResult Index()
        {
            var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            ViewBag.ip_id = ip_id;

            return View("Dashboard");
        }

        public ActionResult UploadDQA()
        {
            var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            ViewBag.ip_id = ip_id;

            return View();
        }

        public ActionResult IpDQA()
        {
            var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            ViewBag.ip_id = ip_id;

            return View();
        }

        public ActionResult GetDQA(int id)
        {
            ViewBag.metadataId = id;
            return View();
        }
    }
}
