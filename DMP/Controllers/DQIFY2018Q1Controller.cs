using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQI.DAL.Services;
using ShieldPortal.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class DQIFY2018Q1Controller : Controller
    {
        // GET: DQIFY2018Q1
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadDQIResult()
        {
            return View();
        }

        //[HttpPost]
        //public string ProcessFile()
        //{ 
        //    string result = "";
        //    if (Request.Files.Count == 0 || string.IsNullOrEmpty(Request.Files[0].FileName))
        //    {  
        //        result = "No file was uploaded";
        //    }
        //    else
        //    {
        //        Profile loggedinProfile = new Utils().GetloggedInProfile();
        //        string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQI/Q1 FY18/" + loggedinProfile.Organization.ShortName) + "\\";
        //        if (Directory.Exists(directory) == false)
        //        {
        //            Directory.CreateDirectory(directory);
        //        }

        //        Request.Files[0].SaveAs(directory + DateTime.Now.ToString("dd_MMM_yyyyh") + Request.Files[0].FileName);

        //        Stream uploadedFile = Request.Files[0].InputStream;
        //        bool status = new QIEngine().ProcessUpload(uploadedFile, "Q1 FY18", loggedinProfile, out result);
        //    }
        //    return result;
        //}

        public ActionResult DownloadDQITool()
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            var DQISites = new QIEngine().GetQISites("Q1 FY18", ip);
            
            return View(DQISites);
        }



        [HttpPost]
        public JsonResult DownloadIPDQITool(DQI.DAL.Model.IPLevelDQI data)
        {
            string fileName = "DQI_Q1_FY18_" + data.IP + ".xlsm";
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQI/Q1 FY18/");
            string template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQI/Q1 FY18/DQI TOOL_unlocked.xlsm");

            new QIEngine().PopulateTool(data, directory, fileName, template);

            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string ProcessFile()
        {
            var messages = "";
            try
            {
                Logger.LogInfo("DQIQ1 FY2018,ProcessFile", "processing dqi upload");

                HttpResponseMessage result = null;
                var userUploading = new Services.Utils().GetloggedInProfile();

                if (Request.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in Request.Files)
                    {
                        var postedFile = Request.Files[file];
                        string ext = Path.GetExtension(postedFile.FileName).Substring(1);

                        if (ext.ToUpper() == "XLS" || ext.ToUpper() == "XLSX" || ext.ToUpper() == "XLSM" || ext.ToUpper() == "ZIP")
                        {

                            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/DQI Q1 FY18/" + postedFile.FileName);
                            postedFile.SaveAs(filePath);

                            messages += new QIEngine().ProcessUpload(filePath, userUploading);
                        }
                        else
                        {
                            messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td><strong>" + postedFile.FileName + "</strong> could not be processed. File is not an excel spreadsheet</td></tr>";
                        }
                    }
                }
                else
                {
                    result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>System error has occurred</td></tr>";
            }
            return messages;
        }
    } 
}