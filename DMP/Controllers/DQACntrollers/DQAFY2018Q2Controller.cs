using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Business;
using DQA.DAL.Model;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using ShieldPortal.Services;
using ShieldPortal.ViewModel;
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
    public class DQAFY2018Q2Controller : Controller
    {
        public ActionResult Index()
        {
            int ip_id = 0;
            if (User.IsInRole("ip"))
            {
                ip_id = new Utils().GetloggedInProfile().Organization.Id;
            }
            ViewBag.ip_id = ip_id;
            return View("Dashboard");
        }

        public ActionResult UploadPivotTable()
        {
            var profile = new Utils().GetloggedInProfile();            
            int ip_id = 0;
            if (User.IsInRole("ip"))
            {
                ip_id = profile.Organization.Id;
            }
            List<UploadList> previousUploads = Utility.RetrievePivotTables(ip_id, "Q2 FY18");
            return View(previousUploads);
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
                Profile loggedinProfile = new Utils().GetloggedInProfile();
                string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/Pivot table " + reportPeriod + "/"
                    + loggedinProfile.Organization.ShortName) + "/";
                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }
                Request.Files[0].SaveAs(directory + Request.Files[0].FileName);

                Stream uploadedFile = Request.Files[0].InputStream;
                bool status = new BDQAQ2FY18().ReadPivotTable(uploadedFile, reportPeriod, loggedinProfile, out result);
            }
            return result;
        }


        public ActionResult DQAResult()
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            var reports = Utility.GetUploadReport("Q2 FY18", ip);
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

        [HttpPost]
        public string GetDQAUploadReport(int? draw, int? start, int? length)
        {
            //string ReportPeriod, int IP
            var search = Request["search[value]"];
            RADET.DAL.Models.RadetMetaDataSearchModel searchModel = JsonConvert.DeserializeObject<RADET.DAL.Models.RadetMetaDataSearchModel>(search);

            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Utils().GetloggedInProfile();
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
            var datatable = Utility.GetFY18Analysis(ip, "Q2 FY18", reportType.ToLower().Contains("partners"));
            var data = new HighChartDataServices().GetConcurrency(datatable, reportType, ip);
            return View(data);
        }

        [HttpPost]
        public string PostDQAFile()
        {
            var messages = "";
            try
            {
                Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
                Logger.LogInfo(" DQAQ2 FY2018,PostDQAFile", "processing dqa upload");

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
                            string directory = System.Web.Hosting.HostingEnvironment
                                .MapPath("~/Report/Uploads/DQA Q2 FY18/" 
                                + loggedinProfile.Organization.ShortName) + "/";
                            if (Directory.Exists(directory) == false)
                            {
                                Directory.CreateDirectory(directory);
                            }

                            var filePath = directory + postedFile.FileName;
                            Request.Files[0].SaveAs(filePath);

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
                                                messages += new BDQAQ2FY18().ReadWorkbook(extractedFilePath, loggedinProfile);
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
                                messages += new BDQAQ2FY18().ReadWorkbook(filePath, loggedinProfile);
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
         
        public ActionResult DQAAnalysisReport(string type)
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }
            var cmd = new SqlCommand();
            cmd.CommandText = "get_FY18_analysis_report_by_quarter";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@reportPeriod", "Q2 FY18");
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@get_partner_report", type.ToLower().Contains("partners"));
            var data = Utility.GetDatable(cmd);

            string JSONresult;
            JSONresult = JsonConvert.SerializeObject(data);
            var convertedResult = JsonConvert.DeserializeObject<List<DQAAnalysisModel>>(JSONresult);

            return View(convertedResult);
        }

       //this is comparison chart between dqa done by umb and the partners
        public ActionResult UMB_Partner_Report()
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            var partner_datatable = Utility.GetFY18Analysis(ip,"Q2 FY18", true);
            var umb_datatable = Utility.GetFY18Analysis(ip, "Q2 FY18", false);
            var Partners_data = new HighChartDataServices().GetConcurrency(partner_datatable, "Partners", ip);
            var UMB_data = new HighChartDataServices().GetConcurrency(umb_datatable, "umb", ip);
            //var Partners_data = new HighChartDataServices().GetQ1HTCConcurrency("Partners", ip);
            //var UMB_data = new HighChartDataServices().GetQ1HTCConcurrency("umb", ip);

            List<ViewModel.DQACompariosnModelMain> mainData = new List<ViewModel.DQACompariosnModelMain>();
             
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.htc_drilldown, UMB_data.AllDataModel.htc_drilldown, "HTC_TST"));
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.pmtct_stat_drilldown, UMB_data.AllDataModel.pmtct_stat_drilldown, "PMTCT_STAT"));
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.pmtct_art_drilldown, UMB_data.AllDataModel.pmtct_art_drilldown, "PMTCT_ART"));
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.pmtct_eid_drilldown, UMB_data.AllDataModel.pmtct_eid_drilldown, "PMTCT_EID"));
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.pmtct_hei_pos_drilldown, UMB_data.AllDataModel.pmtct_hei_pos_drilldown, "PMTCT_HEI_POS"));
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.tx_new_drilldown, UMB_data.AllDataModel.tx_new_drilldown, "TX_NEW"));
            mainData.AddRange(AppendComparisonList(Partners_data.AllDataModel.tx_curr_drilldown, UMB_data.AllDataModel.tx_curr_drilldown, "TX_CURR"));


            ViewBag.mainData = mainData;
            return View(Partners_data);
        }

        List<ViewModel.DQACompariosnModelMain> AppendComparisonList(List<ViewModel.ChildSeriesData> Partnersdata, List<ViewModel.ChildSeriesData> UMBdata, string type)
        {
            List<ViewModel.DQACompariosnModelMain> mainData = new List<ViewModel.DQACompariosnModelMain>();

            var ip_htc_group = Partnersdata.GroupBy(x => x.name);
            foreach (var k in ip_htc_group)
            {
                Dictionary<string, double> umb_facility_numbers = new Dictionary<string, double>();
                var umb_grouping = UMBdata.Where(x => x.name == k.Key).ToList();
                if (umb_grouping != null)
                {
                    umb_facility_numbers = ConvertToDoubleList(umb_grouping.SelectMany(x => x.data.SelectMany(y => y)).ToList());
                }

                var i = k.ToList().SelectMany(x => x.data.SelectMany(y => y)).ToList();
                var partner_facility_numbers = ConvertToDoubleList(i);

                List<string> sites = new List<string>();
                List<double> _partnerNumbers = new List<double>();
                List<double> _umb_numbers = new List<double>();

                foreach (var d in umb_facility_numbers.Keys)
                {
                    if (partner_facility_numbers.ContainsKey(d))
                    {
                        sites.Add(d);
                        _partnerNumbers.Add(partner_facility_numbers[d]);
                        _umb_numbers.Add(umb_facility_numbers[d]);
                    }
                }

                mainData.Add(new ViewModel.DQACompariosnModelMain
                {
                    indicator = type,
                    Sites = sites, //partner_nums.Select(x => x.Key).ToList(),
                    data = new List<ViewModel.DQAComparisonModel>
                    {
                        new ViewModel.DQAComparisonModel
                      {
                           data = _partnerNumbers, //partner_nums.Select(x=>x.Value).ToList(),
                            Type = k.Key,
                      },
                        new ViewModel.DQAComparisonModel
                      {
                           data = _umb_numbers, //umb_nums.Select(x=>x.Value).ToList(),
                            Type = "UMB",
                      }
                    }
                });
            }

            return mainData;
        }


        private Dictionary<string, double> ConvertToDoubleList(List<object> input)
        {
            Dictionary<string, double> d = new Dictionary<string, double>();
            for (int i = 0; i < input.Count;)
            {
                d.Add(Convert.ToString(input[i]), Math.Round(Convert.ToDouble(input[i + 1]), 2)); 
                i += 2;
            }
            return d;
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



        [HttpGet]
        public string GetReportDetails(int metadataid)
        {
            string Processed_result = new BDQAQ2FY18().GetReportDetails(metadataid);
            return Processed_result;
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
            string currentPeriod = "Q2 FY18";

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
            string file = await new BDQAQ2FY18().GenerateDQA(data, profile.Organization);

            return Json(file, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> DownloadDQAResource(string fileType)
        {
            string filename = "";
            switch (fileType)
            {
                case "DQAUserGuide":
                    filename = "Data Quality Assessment Userguide Q2 FY18.pdf";
                    break;
                case "PivotTable":
                    filename = "DATIM PIVOT TABLE Q2 FY18.xlsx";
                    break;
                case "SummarySheet":
                    filename = "Summary Sheet FY18 Q2 DQA.pdf";
                    break;
            }
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2018 Q2/" + filename);

            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
            }
        }

        public ActionResult RadetValidation()
        {
            string period = "Q2 FY18";
            string ip = "";
            var profile = new Utils().GetloggedInProfile();
            if (User.IsInRole("ip"))
                ip = profile.Organization.ShortName;

            var pivot_data = Utility.RetrievePivotTablesForComparison(new List<string> { ip }, period);
            var iplocation = (from pvt in pivot_data
                              select new
                              {
                                  pvt.FacilityName,
                                  pvt.IP,
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

            string period = "Q2 FY18";
            string startDate = "2018-01-01 00:00:00.000";
            string endDate = "2018-03-31 23:59:59.000";

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
                        State = item.TheLGA.state.state_name,
                        LGA = $"{item.TheLGA.lga_name}",
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