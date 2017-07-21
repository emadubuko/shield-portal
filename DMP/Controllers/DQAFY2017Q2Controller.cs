using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Business;
using DQA.DAL.Model;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
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

        [HttpPost]
        public string PostDQAFile()
        {
            var messages = "";
            try
            {
                Logger.LogInfo(" DQAAPi,post", "processing dqa upload");

                HttpResponseMessage result = null;
           
                if (Request.Files.Count > 0)
                {
                    var docfiles = new List<string>();
                    foreach (string file in Request.Files)
                    {
                        var postedFile = Request.Files[file];
                        string ext = Path.GetExtension(postedFile.FileName).Substring(1);

                        if (ext.ToUpper() == "XLS" || ext.ToUpper() == "XLSX" || ext.ToUpper() == "XLSM" || ext.ToUpper() == "ZIP")
                        {

                            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/DQA Q3 FY16/" + postedFile.FileName);
                            postedFile.SaveAs(filePath);

                            //get the datim file containing the DQA numbers for all the facilities
                            string DatimFileSource = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DatimSource.csv");
                            string[] datimNumbersRaw = System.IO.File.ReadAllText(DatimFileSource).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                            if (ext.ToUpper() == "ZIP")
                            {
                                messages += "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td><strong>" + postedFile.FileName + "</strong> : Decompressing please wait.</td></tr>";
                                var countFailed = 0;
                                try
                                {
                                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                                    using (ZipFile zipFile = new ZipFile(fs))
                                    {

                                        var countProcessed = 0;

                                        var countSuccess = 0;
                                        var step = 0;
                                        var total = (int)zipFile.Count;
                                        var currentFile = "";


                                        foreach (ZipEntry zipEntry in zipFile)
                                        {
                                            step++;

                                            if (!zipEntry.IsFile)
                                            {
                                                continue;
                                            }
                                            currentFile = zipEntry.Name;
                                            var entryFileName = zipEntry.Name;
                                            var extractedFilePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/tempData/"), entryFileName);
                                            var extractedDirectory = Path.GetDirectoryName(extractedFilePath);
                                            var entryExt = Path.GetExtension(extractedFilePath).Substring(1);


                                            if (extractedDirectory.Length > 0)
                                            {
                                                Directory.CreateDirectory(extractedDirectory);
                                            }

                                            if (entryExt.ToUpper() == "XLS" || entryExt.ToUpper() == "XLSX" || entryExt.ToUpper() == "XLSM")
                                            {
                                                countProcessed++;
                                                Stream zipStream = zipFile.GetInputStream(zipEntry);
                                                using (FileStream entryStream = System.IO.File.Create(extractedFilePath))
                                                {
                                                    StreamUtils.Copy(zipStream, entryStream, new byte[4096]);
                                                }
                                                messages += new BDQAQ2().ReadWorkbook(extractedFilePath, User.Identity.Name, datimNumbersRaw);
                                                countSuccess++;
                                            }

                                        }
                                        zipFile.IsStreamOwner = true;
                                        zipFile.Close();
                                    }
                                }
                                catch (Exception exp)
                                {
                                    Logger.LogError(exp);
                                    countFailed++;
                                    messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td><strong>" + postedFile.FileName + "</strong>: An Error occured. Please check the files.</td></tr>";
                                }
                            }
                            else
                            {
                                messages += new BDQAQ2().ReadWorkbook(filePath, User.Identity.Name, datimNumbersRaw);
                            }
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

        //this is the page
        public ActionResult DownloadDQATool()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<PivotTableModel> sites = new List<PivotTableModel>();
            string currentPeriod = System.Configuration.ConfigurationManager.AppSettings["ReportPeriod"];

            if (User.IsInRole("ip"))
                sites = Utility.RetrievePivotTablesForDQATool(profile.Organization.Id, currentPeriod);
            else
                sites = Utility.RetrievePivotTablesForDQATool(0, currentPeriod);
            return View(sites);
        }

        //this is for actual downloading of the file
        [HttpPost]
        public async Task<ActionResult> DownloadDQATool(List<PivotTableModel> data)
        {            
            var profile = new Services.Utils().GetloggedInProfile();
            //the radet period is on the config file
            string file = await new BDQAQ2().GenerateDQA(data, profile.Organization);

            return Json(file, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> downloadDQAResource(string fileType)
        {
            string filename = "";
            switch (fileType)
            { 
                case "DQAUserGuide":
                    filename = "THE DATA QUALITY ASSESSMENT USER GUIDE FOR FY17 Q3.pdf";
                    break;
                case "PivotTable":
                    filename = "DATIM PIVOT TABLE SAMPLE.xlsx";
                    break;
                case "SummarySheet":
                    filename = "Printable Summary sheet DQA_Q3R.pdf";
                    break;
            }
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q3/" + filename);

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