using CommonUtil.Utilities;
using DQI.DAL.Model;
using DQI.DAL.Services;
using ShieldPortal.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class DQIFY2019Q2Controller : Controller
    {
        // GET: DQIFY2019Q2

        public ActionResult Index()
            {
                var loggedinUser = new Utils().GetloggedInProfile();
                var reports = new QIEngine().RetrieveUpload(loggedinUser, "Q2 FY19");
                return View(reports);
            }

        [HttpPost]
        public JsonResult RetriveDetails(int id)
        {
            var item = new QIEngine().RetrieveUpload(id);
            return Json(new
            {
                item.ImplementingPartner,
                item.DqaIndicator,
                item.AffectedFacilityNumber,
                item.ImprovementApproach,
                item.DataCollectionMethod,
                item.Problem,
                //item.pr
                item.ProblemResolved,
                WhyDoesProblemOccur = item.WhyDoesProblemOccur.Replace("<i>", "i. ").Replace("<ii>", "ii. ").Replace("<iii>", "iii. ").Replace("<iv>", "iv. ").Replace("<v>", "v. ").Replace("<i>", "i. ").Replace("</i>", "<br />").Replace("</ii>", "<br />").Replace("</iii>", "<br />").Replace("</iv>", "<br />").Replace("</v>", "<br />"),
                item.ImprovementApproach_Analyze,
                Interventions = item.Interventions.Replace("<i>", "i. ").Replace("<ii>", "ii. ").Replace("<iii>", "iii. ").Replace("<iv>", "iv. ").Replace("<v>", "v. ").Replace("<i>", "i. ").Replace("</i>", "<br />").Replace("</ii>", "<br />").Replace("</iii>", "<br />").Replace("</iv>", "<br />").Replace("</v>", "<br />"),
                item.ImprovementApproach_Develop,
                ProcessTracking = item.ProcessTracking.Replace("<i>", "i. ").Replace("<ii>", "ii. ").Replace("<iii>", "iii. ").Replace("<iv>", "iv. ").Replace("<v>", "v. ").Replace("<i>", "i. ").Replace("</i>", "<br />").Replace("</ii>", "<br />").Replace("</iii>", "<br />").Replace("</iv>", "<br />").Replace("</v>", "<br />"),
                item.MeasureIndicators,
                item.EvaluateIndicators,
                Indicators = XMLUtil.FromXml<List<ProcessTable>>(item.Indicators) //XMLUtil.FromXml<Reported_data>(item.Indicators)
            });
        }

        public ActionResult UploadDQIResult()
        {
            return View();
        }

        public ActionResult DownloadDQITool()
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            var DQISites = new QIEngine().GetQISites("Q2 FY19", ip);

            return View(DQISites);
        }



        [HttpPost]
        public JsonResult DownloadIPDQITool(DQI.DAL.Model.IPLevelDQI data)
        {
            string fileName = "DQI_Q2_FY19_" + data.IP + ".xlsm";
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQI/Q2 FY19/");
            string template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQI/Q2 FY19/DQI TOOL_unlocked.xlsm");

            new QIEngine().PopulateTool(data, directory, fileName, template);

            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string ProcessFile()
        {
            var messages = "";
            try
            {
                Logger.LogInfo("DQIQ1 FY2019,ProcessFile", "processing dqi upload");

                var userUploading = new Services.Utils().GetloggedInProfile();

                if (Request.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in Request.Files)
                    {
                        var postedFile = Request.Files[file];
                        string ext = Path.GetExtension(postedFile.FileName).Substring(1);

                        if (ext.ToUpper() == "XLS" || ext.ToUpper() == "XLSX" || ext.ToUpper() == "XLSM")
                        {
                            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/DQI Q1 FY19/" + postedFile.FileName);
                            postedFile.SaveAs(filePath);

                            messages += new QIEngine().ProcessUpload(filePath, userUploading, "Q2 FY19");
                        }
                        else
                        {
                            messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td><strong>" + postedFile.FileName + "</strong> could not be processed. File is not an excel spreadsheet</td></tr>";
                        }
                    }
                }
                else
                {
                    //result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    return "00|File uploaded successfully";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                messages += ex.Message;
            }
            return messages;
        }
    }
}