using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Business;
using DQA.DAL.Model;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using ShieldPortal.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class DQAFY2017Q4Controller : Controller
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

        public ActionResult UploadPivotTable()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<UploadList> previousUploads = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = Utility.RetrievePivotTables(0, "Q4 FY17");
            }
            else
            {
                previousUploads = Utility.RetrievePivotTables(profile.Organization.Id, "Q4 FY17");
            }
            return View(previousUploads);
        }

        public ActionResult DeletePivotTable(int id)
        {
            new BDQAQ4FY17().DeletePivotTable(id);
            return RedirectToAction("UploadPivotTable");
        }

        [HttpPost]
        public string ProcesssPivotTable(string reportPeriod)
        {
            HttpResponseMessage msg = null;
            string result = "";
            if (Request.Files.Count == 0 || string.IsNullOrEmpty(Request.Files[0].FileName))
            {
                msg = new HttpResponseMessage(HttpStatusCode.BadRequest); //, );
                result = "No file was uploaded";
            }
            else
            {
                Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
                Request.Files[0].SaveAs(Server.MapPath("~/Report/Uploads/Pivot table Q4/" + loggedinProfile.Organization.ShortName + "_" + Request.Files[0].FileName));
                
                Stream uploadedFile = Request.Files[0].InputStream;
                bool status = new BDQAQ4FY17().ReadPivotTable(uploadedFile, reportPeriod, loggedinProfile, out result);
            }
            return result; //msg;
        }


        [HttpPost]
        public string GetDQAUploadReport(int? draw, int? start, int? length)
        {
            //string ReportPeriod, int IP
            var search = Request["search[value]"];
            RADET.DAL.Models.RadetMetaDataSearchModel searchModel = JsonConvert.DeserializeObject<RADET.DAL.Models.RadetMetaDataSearchModel>(search);

            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }
            else if (searchModel.IPs != null)
            {
                ip = searchModel.IPs.FirstOrDefault();
            }
            var reports = Utility.GetUploadReport(searchModel.RadetPeriod, ip, searchModel.state_codes, searchModel.lga_codes, searchModel.facilities);
            List<dynamic> mydata = new List<dynamic>();
            foreach (DataRow dr in reports.Rows)
            {
                var dtt = new
                {
                    DT_RowId = dr[9].ToString(),
                    //DT_RowClass = "click-row",
                    IP = dr[0],
                    State = dr[1],
                    LGA = dr[3],
                    Facility = dr[5],
                    ReportPeriod = dr[6],
                    Uploaded_Date = dr[7],
                    UploadedBy = dr[8],
                    DoneBy = dr[10].ToString() == "ip" ? dr[0] : "UMB",
                    lga_code = dr[4],
                    state_code = dr[2],
                    LastColumn = string.Format("<td>&nbsp;&nbsp;<a style ='text-transform: capitalize;' class='btn btn-sm btn-danger deletebtn' id='{0}'><i class='fa fa-trash'></i>&nbsp;&nbsp;Delete</a> <i style='display:none' id='loadImg{0}'><img class='center' src='/images/spinner.gif' width='40'> please wait ...</i></td>", dr[9])
                };
                mydata.Add(dtt);
            }
            if (searchModel.state_codes != null && searchModel.state_codes.Count > 0)
                mydata = mydata.Where(x => searchModel.state_codes.Contains(x.state_code)).ToList();

            if (searchModel.lga_codes != null && searchModel.lga_codes.Count > 0)
                mydata = mydata.Where(x => searchModel.lga_codes.Contains(x.lga_code)).ToList();

            if (searchModel.facilities != null && searchModel.facilities.Count > 0)
                mydata = mydata.Where(x => searchModel.facilities.Contains(x.Facility)).ToList();

            return JsonConvert.SerializeObject(
                       new
                       {
                           sEcho = draw,
                           iTotalRecords = mydata.Count(),
                           iTotalDisplayRecords = mydata.Count(),
                           aaData = mydata
                       });
        }


        public ActionResult Analytics(string reportType = "Partners")
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }
            var data = new HighChartDataServices().GetQ4Concurrency(reportType, ip);
            return View(data);
        }


        //this is for Q2 upload
        [HttpPost]
        public string PostDQAFile()
        {
            var messages = "";
            try
            {
                Logger.LogInfo(" DQAQ4 FY2017,PostDQAFile", "processing dqa upload");

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

                            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/DQA Q4 FY17/" + postedFile.FileName);
                            postedFile.SaveAs(filePath);

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
                                                messages += new BDQAQ4FY17().ReadWorkbook(extractedFilePath, userUploading);
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
                                messages += new BDQAQ4FY17().ReadWorkbook(filePath, userUploading);
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



        [HttpGet]
        public string GetDashboardStatistic()
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }
            var ds = Utility.GetDashboardStatistic(ip, "Q4 FY17");
            List<dynamic> IPSummary = new List<dynamic>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                IPSummary.Add(new
                {
                    Name = dr[0],
                    Submitted = dr[1],
                    Pending = dr[2],
                    Total = dr[3],
                });
            }

            return JsonConvert.SerializeObject(new
            {
                IPSummary,
                cardData = ds.Tables[1].Rows[0].ItemArray
            }); 
        }

        //Q4 Fy17
        public ActionResult DQAAnalysisReport(string type)
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }
            var cmd = new SqlCommand();
            cmd.CommandText = "get_q4_FY17_analysis_report";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@get_partner_report", type.ToLower().Contains("partners"));
            var data = Utility.GetDatable(cmd);
             

            return View(data);
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
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            var reports = Utility.GetUploadReport("Q4 FY17", ip);
            List<dynamic> iplocation = new List<dynamic>();
            foreach (DataRow dr in reports.Rows)
            {
                iplocation.Add(new
                {
                    IP = dr[0],
                    FacilityName = dr[5],
                    LGA = new
                    {
                        lga_name = dr[3],
                        lga_code = dr[4],
                        state_code = dr[2],
                        DisplayName = string.Format("{0} ({1})", dr[3], dr[1]),
                        State = new { state_name = dr[1], state_code = dr[2] }
                    }
                });
            }
            ViewBag.selectModel = JsonConvert.SerializeObject((iplocation), Formatting.None,
                       new JsonSerializerSettings()
                       {
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                           ContractResolver = new NHibernateContractResolver()
                       });
            ViewBag.IPCount = iplocation.Select(x => x.IP).Distinct().Count();
            return View();
        }

        

        [HttpGet]
        public string GetReportDetails(int metadataid)
        {
            return new BDQAQ4FY17().GetReportDetails(metadataid);
        }

        public void PopulateStates(object selectStatus = null)
        {
            var statusQuery = new BaseDAO<State, long>().RetrieveAll();
            ViewBag.states = new SelectList(statusQuery, "state_code", "state_name", selectStatus);
        }

        

        //this is the page
        public ActionResult DownloadDQATool(int? ip)
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<PivotTableModel> sites = new List<PivotTableModel>();
            string currentPeriod = "Q4 FY17";

            int getIP = 0;
            if (User.IsInRole("ip"))
            {
                getIP = profile.Organization.Id;
            }
            else if (ip.HasValue)
            {
                getIP = ip.Value;
            }
            sites = Utility.RetrievePivotTablesForDQATool(getIP, currentPeriod);
            return View(sites);
        }

        //this is for actual downloading of the file
        [HttpPost]
        public async Task<ActionResult> DownloadDQATool(List<PivotTableModel> data)
        {
            var profile = new Services.Utils().GetloggedInProfile();
            //the radet period is on the config file
            string file = await new BDQAQ4FY17().GenerateDQA(data, profile.Organization);

            return Json(file, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> downloadDQAResource(string fileType)
        {
            string filename = "";
            switch (fileType)
            {
                case "DQAUserGuide":
                    filename = "Data Quality Assessment Userguide Q4.pdf";
                    break;
                case "PivotTable":
                    filename = "DATIM PIVOT TABLE Q4.xlsx";
                    break;
                case "SummarySheet":
                    filename = "Summary Sheet Q4 DQA.pdf";
                    break;
            }
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q4/" + filename);

            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
        }

        public ActionResult RadetValidation()
        {
            string period = "Q4 FY17"; 
            string ip = "";
            var profile = new Services.Utils().GetloggedInProfile();
            if (User.IsInRole("ip"))
                ip = profile.Organization.ShortName;

            var pivot_data = Utility.RetrievePivotTablesForComparison(new List<string> { ip }, period);
            var iplocation = (from pvt in pivot_data
                              select new
                              {
                                  FacilityName = pvt.FacilityName,
                                  IP = pvt.IP,
                                  LGA = new
                                  {
                                      pvt.TheLGA.lga_code,
                                      pvt.TheLGA.lga_name,
                                      pvt.TheLGA.state_code,
                                      DisplayName = string.Format("{0} ({1})", pvt.TheLGA.lga_name, pvt.TheLGA.state.state_name),
                                      State = new { pvt.TheLGA.state.state_name, pvt.TheLGA.state.state_code }
                                  }
                              });
            var model = new { IPLocation = iplocation };
            ViewBag.selectModel = JsonConvert.SerializeObject((iplocation), Formatting.None,
                       new JsonSerializerSettings()
                       {
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                           ContractResolver = new NHibernateContractResolver()
                       });
            ViewBag.IPCount = pivot_data.Select(x => x.IP).Distinct().Count();
            return View();
        }

        [HttpPost]
        public string RadetValidationData(int? draw, int? start, int? length)
        {
            var search = Request["search[value]"];
            RADET.DAL.Models.RadetMetaDataSearchModel searchModel = JsonConvert.DeserializeObject<RADET.DAL.Models.RadetMetaDataSearchModel>(search);

            string period = "Q4 FY17";
            string startDate = "2017-07-01 00:00:00.000";
            string endDate = "2017-09-30 23:59:59.000";
            
            string ip = "";
            var profile = new Services.Utils().GetloggedInProfile();
            if (User.IsInRole("ip"))
                ip = profile.Organization.ShortName;
            else
            {
                ip = searchModel.IPs.FirstOrDefault();
            }
            var radet_data = Utility.GetRADETNumbers(ip, startDate, endDate, period);
            var pivot_data = Utility.RetrievePivotTablesForComparison(new List<string> { ip }, period, searchModel.state_codes, searchModel.lga_codes, searchModel.facilities);
            var artSites = Utilities.GetARTSiteWithDATIMCode();

            List<dynamic> mydata = new List<dynamic>();
            List<dynamic> mydata2 = new List<dynamic>();

            foreach (DataRow dr in radet_data.Rows)
            {
                var dtt = new
                {
                    ShortName = dr[0],
                    Facility = dr[1],
                    Tx_New = dr[2],
                    Tx_Curr = dr[3],
                };
                mydata.Add(dtt);
            }
            foreach (var item in pivot_data)
            {
                string radetSite;
                artSites.TryGetValue(item.FacilityCode, out radetSite);
                if (string.IsNullOrEmpty(radetSite))
                {
                    radetSite = item.FacilityName;
                }
                var r_data = mydata.FirstOrDefault(x => x.ShortName == item.IP && x.Facility == radetSite);
                if (r_data != null)
                {
                    int tx_new = item.TX_NEW.HasValue ? item.TX_NEW.Value : 0;
                    mydata2.Add(new
                    {
                        ShortName = item.IP,
                        Facility = item.FacilityName,
                        Tx_New = r_data.Tx_New,
                        p_Tx_New = item.TX_NEW,
                        Tx_New_difference = Math.Abs((int)r_data.Tx_New - tx_new),
                        Tx_New_concurrency = 100 * Math.Abs((int)r_data.Tx_New - tx_new) / (int)r_data.Tx_New,

                        Tx_Curr = r_data.Tx_Curr,
                        p_Tx_Curr = item.TX_CURR,
                        Tx_Curr_difference = Math.Abs((int)r_data.Tx_Curr - item.TX_CURR),
                        Tx_Curr_concurrency = 100 * Math.Abs((int)r_data.Tx_Curr - item.TX_CURR) / (int)r_data.Tx_Curr,
                    });
                } 
            } 
            return JsonConvert.SerializeObject(
                       new
                       {
                           sEcho = draw,
                           iTotalRecords = mydata2.Count(),
                           iTotalDisplayRecords = mydata2.Count(),
                           aaData = mydata2
                       });
        }
    }
}