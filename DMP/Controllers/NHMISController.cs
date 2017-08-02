using NHMIS.DAL.BizModel;
using NHMIS.DAL.Processors;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class NHMISController : Controller
    {
        // GET: NHMIS
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Upload()
        {
            return View();
        }

        public JsonResult ProcessFileUpload()
        {
            var pivot = Request.Files["up_pivot"];
            var nhrs = Request.Files["NHSR"];

           new NHMISProcessor().processFile(pivot.InputStream, nhrs.InputStream);

            return Json("");
        }
    }

    
}