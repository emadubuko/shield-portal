using CommonUtil.Entities;
using CommonUtil.Utilities;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RADET.DAL.DAO;
using RADET.DAL.Entities;
using RADET.DAL.Models;
using RADET.DAL.Services;
using ShieldPortal.Models;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ShieldPortal.Controllers
{
    [System.Web.Mvc.Authorize]
    public class RadetAnalyticsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RADETScoreCard()
        {
            return View();
        }

        [HttpPost]
        public string RADETScoreCardData()
        {
            string period = System.Configuration.ConfigurationManager.AppSettings["ReportPeriod"];  

            var data = new RadetMetaDataDAO().GetScoreCard(period);

            List<dynamic> mydata = new List<dynamic>();

            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                foreach (DataRow dr in data.Rows)
                {
                    var dtt = new
                    {
                        ShortName = dr[0],
                        RadetPeriod = dr[1],
                        FacilityCount = dr[2],
                        ARTSites = dr[3]

                    };
                    mydata.Add(dtt);
                }
            }
            else
            {
                var profile = new Services.Utils().GetloggedInProfile();
                foreach (DataRow dr in data.Rows)
                {
                    if (dr[0].ToString() == profile.Organization.ShortName)
                    {
                        var dtt = new
                        {
                            ShortName = dr[0],
                            RadetPeriod = dr[1],
                            FacilityCount = dr[2],
                            ARTSites = dr[3]

                        };
                        mydata.Add(dtt);
                    }
                }
            }

            return JsonConvert.SerializeObject(mydata);
        }

        public ActionResult UploadPage()
        {
            
            var profile = new Services.Utils().GetloggedInProfile();

            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                ViewBag.showdelete = true;
                ViewBag.Orgs = new CommonUtil.DAO.OrganizationDAO().RetrieveAll().Where(x => x.ShortName != "MGIC" && x.ShortName != "CDC");
            }
            else
            {
                ViewBag.Orgs = new List<Organizations> { profile.Organization };
                ViewBag.showdelete = false;
            }
            return View();
        }

        public ActionResult ViewPreviousUploads()
        {
            List<RadetReportModel2> list = GetPreviousUpload();

            RandomizerDropDownModel model = new RandomizerDropDownModel();
            model.IPLocation = new List<IPLGAFacility>();
            model.AllowCriteria = (User.IsInRole("shield_team") || User.IsInRole("sys_admin"));

            foreach (var rdt in list)
            {
                if (!model.IPLocation.Any(a => a.IP == rdt.IP && a.FacilityName == rdt.Facility && a.LGA == rdt.LGA && a.RadetPeriod == rdt.RadetPeriod))
                {
                    model.IPLocation.Add(
                new IPLGAFacility
                {
                    FacilityName = rdt.Facility,
                    IP = rdt.IP,
                    LGA = rdt.LGA,
                    RadetPeriod = rdt.RadetPeriod
                });
                }
            }

            ViewBag.selectModel = model;

            return View(list);
        }

        [HttpPost]
        public string SearchRadet(int? draw, int? start, int? length)
        {
            var search = Request["search[value]"];
            var totalRecords = 0;
            var recordsFiltered = 0;
            start = start.HasValue ? start / 10 : 0;
            RadetMetaDataSearchModel searchModel = JsonConvert.DeserializeObject<RadetMetaDataSearchModel>(search);
            if (User.IsInRole("ip"))
            {
                searchModel.IPs = new List<string> { new Services.Utils().GetloggedInProfile().Organization.ShortName };
            }
         
            RadetMetaDataDAO _dao = new RadetMetaDataDAO();
            var list = new RadetMetaDataDAO().RetrieveUsingPaging(searchModel, start.Value, length ?? 20, false, out totalRecords);
             
            recordsFiltered = list.Count();

            list.ForEach(x =>
            {

                x.FirstColumn = string.Format("<input type='checkbox' id='{0}' class='chcktbl' />", x.Id);
                x.LastColumn = string.Format("<td><a style='text-transform: capitalize;' class='btn btn-sm btn-info viewPatientListing' id='{0}'>View Entries</a>&nbsp;&nbsp;&nbsp;<a style ='text-transform: capitalize;' class='btn btn-sm btn-danger deletebtn' id='{0}'><i class='fa fa-trash'></i>&nbsp;&nbsp;Delete</a> <i style='display:none' id='loadImg2'><img class='center' src='/images/spinner.gif' width='40'> please wait ...</i></td>", x.Id);
            });

            return JsonConvert.SerializeObject(
                        new
                        {
                            sEcho = draw,
                            recordsTotal = totalRecords,
                            recordsFiltered = totalRecords,
                            data = list
                        });
        }

        public ActionResult Randomizer()
        {
            IList<RadetMetaData> RadetMetadata = null;
            List<string> ips = null;

            if (!User.IsInRole("shield_team") && !(User.IsInRole("sys_admin")))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ips = new List<string> { profile.Organization.ShortName };
            }

            RadetMetadata = new RadetMetaDataDAO().SearchRadetData(ips, null, null, "");

            RandomizerDropDownModel model = new RandomizerDropDownModel();
            model.IPLocation = new List<IPLGAFacility>();
            model.AllowCriteria = (User.IsInRole("shield_team") || User.IsInRole("sys_admin"));

            foreach (var rdt in RadetMetadata)
            {
                if (!model.IPLocation.Any(a => a.IP == rdt.IP.ShortName && a.FacilityName == rdt.Facility && a.LGA == rdt.LGA && a.RadetPeriod == rdt.RadetPeriod))
                {
                    model.IPLocation.Add(
                new IPLGAFacility
                {
                    FacilityName = rdt.Facility,
                    IP = rdt.IP.ShortName,
                    LGA = rdt.LGA,
                    RadetPeriod = rdt.RadetPeriod
                });
                }
            }
            return View(model);

            //var ip = RadetMetadata.Select(x => x.IP.ShortName).Distinct();
            //var facility = RadetMetadata.Select(x => x.Facility).Distinct();
            //var radetPeriod = RadetMetadata.Select(x => x.RadetPeriod).Distinct();
            //var lga = RadetMetadata.Select(x => x.LGA).Distinct();
            //return View(new RandomizerModel
            //{
            //    LGA = lga,
            //    RadetPeriod = radetPeriod,
            //    Facility = facility,
            //    IP = ip,
            //    AllowCriteria = (User.IsInRole("shield_team") || User.IsInRole("sys_admin"))
            //});
        }

        /// <summary>
        /// from the select page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RandomizeRadet(RadetMetaDataSearchModel model)//(int id, int active, int inactive)
        {
            IList<RandomizationUpdateModel> list = null;
            RadetMetaDataDAO _dao = new RadetMetaDataDAO();
            if (model != null && model.MetaDataId == 0)
            {

                list = new RadetMetaDataDAO().SearchPatientLineListing(model.IPs, model.lga_codes, model.facilities, model.state_codes, model.RadetPeriod);
            }
            else
            {
                list = (from item in _dao.SearchPatientLineListing()
                        where item.Id == model.MetaDataId
                        select new RandomizationUpdateModel
                        {
                            Id = item.Id,
                            CurrentARTStatus = item.CurrentARTStatus,
                            RandomlySelect = item.RandomlySelect,
                            FacilityName = item.RadetPatient.FacilityName,
                            MetadataId = item.MetaData.Id
                        }).ToList();
            }
            var result = new RADETProcessor().Randomizetems(list, model.Active, model.Inactive);
            HttpContext.Session["downloadableIds"] = result.Where(x => x.RandomlySelect).Select(x => x.MetadataId).Distinct().ToArray();
            return Json(
                new
                {
                    Message = string.Format("{0} records selected out {1};", result.Count(x => x.RandomlySelect), result.Count()),
                },
                JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public HttpResponseMessage DeleteRadet(int id)
        {
            HttpResponseMessage msg = null;
            RadetMetaDataDAO dao = new RadetMetaDataDAO();
            try
            {
                dao.DeleteRecord(id);
                msg = new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                msg = new HttpResponseMessage(HttpStatusCode.BadRequest);
                msg.Content = new StringContent(ex.Message);
            }
            return msg;
        }

        public List<RadetReportModel2> GetPreviousUpload(int IP = 0)
        {
            var profile = new Services.Utils().GetloggedInProfile();
            RadetMetaDataDAO metaDao = new RadetMetaDataDAO();

            IQueryable<RadetMetaData> previousUploads = null;

            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = metaDao.RetrieveRadetUpload(0);
            }
            else
            {
                previousUploads = metaDao.RetrieveRadetUpload(profile.Organization.Id);
            }

            List<RadetReportModel2> list = new List<RadetReportModel2>();
            if (previousUploads != null)
            {
                list = (from entry in previousUploads
                        select new RadetReportModel2
                        {
                            Facility = entry.Facility,
                            IP = entry.IP.ShortName,
                            RadetPeriod = entry.RadetPeriod,
                            UploadedBy = entry.RadetUpload.UploadedBy.FirstName + " " + entry.RadetUpload.UploadedBy.Surname,
                            DateUploaded = entry.RadetUpload.DateUploaded,
                            LGA = entry.LGA,
                            Id = entry.Id,
                        }).ToList();
            }
            return list;
        }



        /// <param name="useSession"></param>
        /// <param name="radetIds"></param>
        /// <returns></returns>
        [Compress]
        public JsonResult ExportData(bool useSession, string radetIds = "")//List<int>
        {
            int[] radetIds_int;
            bool RandomlySelectOnly = false;
            if (useSession == false && !string.IsNullOrEmpty(radetIds))
            {
                radetIds_int = Array.ConvertAll(radetIds.Split(','), int.Parse);
            }
            else
            {
                radetIds_int = HttpContext.Session["downloadableIds"] as int[];
                RandomlySelectOnly = true;
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("IP, State, LGA, Facility, Patient Id,Hospital No,Sex,Age At Start Of ART (In Years),Age At Start Of ART (In Months),ART Start Date,Last Pickup Date,Months Of ARV Refill,Regimen Line At ART Start,Regimen At Start Of ART,Current Regimen Line,Current ART Regimen,Pregnancy Status,Current Viral Load,Date Of Current Viral Load,Viral Load Indication,Current ART Status");

            Action<ExportData> _action = (ExportData pt) =>
            {
                sb.AppendLine(string.Format("{0},{1},{2},\'{3}\',\'{4}\',\'{5}\',{6},{7},{8},{9:dd-MMM-yyyy},{10:dd-MMM-yyyy},{11},\'{12}\',\'{13}\',\'{14}\',\'{15}\',{16},\'{17}\',{18}, \'{19}\', {20}",
                                  pt.IPShortName, pt.State, pt.LGA, pt.Facility.Replace(',', ' '), !string.IsNullOrEmpty(pt.PatientId) ? pt.PatientId : "", !string.IsNullOrEmpty(pt.HospitalNo) ? pt.HospitalNo : "", !string.IsNullOrEmpty(pt.Sex) ? pt.Sex : "",
                                  pt.AgeInYears, pt.AgeInMonths, pt.ARTStartDate, pt.LastPickupDate, pt.MonthsOfARVRefill, !string.IsNullOrEmpty(pt.RegimenLineAtARTStart) ? pt.RegimenLineAtARTStart.Replace(',', ' ') : "", !string.IsNullOrEmpty(pt.RegimenAtStartOfART) ? pt.RegimenAtStartOfART.Replace(',', ' ') : "", !string.IsNullOrEmpty(pt.CurrentRegimenLine) ? pt.CurrentRegimenLine.Replace(',', ' ') : "", !string.IsNullOrEmpty(pt.CurrentRegimenLine) ? pt.CurrentARTRegimen.Replace(',', ' ') : "", pt.PregnancyStatus, !string.IsNullOrEmpty(pt.CurrentViralLoad) ? pt.CurrentViralLoad.Replace(',', ' ') : "", pt.DateOfCurrentViralLoad.HasValue ? pt.DateOfCurrentViralLoad.Value.ToString("dd-MMM-yyyy") : "", pt.ViralLoadIndication, pt.CurrentARTStatus));
            };

            List<ExportData> result = new List<ViewModel.ExportData>();
            const int pageSize = 50;
            int numberOfPages = (radetIds_int.Count() / pageSize) + (radetIds_int.Count() % pageSize == 0 ? 0 : 1);
            //int currentPage = 0;

            for (int currentPage = 0; currentPage < numberOfPages; currentPage++)
            {
                var _d = radetIds_int.Skip(currentPage * pageSize).Take(pageSize).ToList();
                result.AddRange(new RadetMetaDataDAO().RetrieveRadetList<ExportData>(_d, RandomlySelectOnly));
            }

            result.ForEach(_action);

            var dt = Json(sb.ToString());
            dt.MaxJsonLength = int.MaxValue;
            return dt;
        }

        /// <summary>
        /// for pop up
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Compress]
        [AllowAnonymous]
        public JsonResult RetrieveRadet(int id)
        {
            string result = new RadetMetaDataDAO().RetrieveRadetData(id);
            var dt = Json(result);
            dt.MaxJsonLength = int.MaxValue;
            return dt;
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public HttpResponseMessage RadetFileUpload(string connectionId, int ip)
        {
            HttpContext.Session[connectionId] = null;

            HttpResponseMessage msg = null;
            UploadLogError uploadError = null;
            RadetUpload upload = null;
            var stopwatch = new Stopwatch();

            RadetUploadDAO _radetUploadDAO = new RadetUploadDAO();
            var errors = new List<ErrorDetails>();

            if (Request.Files.Count == 0 || string.IsNullOrEmpty(Request.Files[0].FileName))
            {
                msg = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                msg.Content = new StringContent("No file uploaded");

                NotifyPage(connectionId, "no file uploaded",
                    new List<ErrorDetails> { new ErrorDetails {
                    FileName = "no file uploaded", ErrorMessage = "no file uploaded", FileTab="", LineNo="", PatientNo=""
                }});
                return msg;
            }
            else
            {
                NotifyPage(connectionId, "validation");

                stopwatch.Start();

                Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
                var IP = new CommonUtil.DAO.OrganizationDAO().Retrieve(ip);
                if (IP == null)
                {
                    msg = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    msg.Content = new StringContent("invalid IP");

                    NotifyPage(connectionId, "invalid IP selected",
                    new List<ErrorDetails> { new ErrorDetails {
                            FileName = "invalid IP selected", ErrorMessage = "invalid IP selected", FileTab="", LineNo="", PatientNo=""
                        }});
                    return msg;
                }

                var LGAs = new CommonUtil.DAO.LGADao().RetrieveAll();

                var errorDetails = new List<ErrorDetails>();

                upload = new RadetUpload
                {
                    DateUploaded = DateTime.Now,
                    IP = IP,
                    UploadedBy = loggedinProfile,
                    RadetMetaData = new List<RadetMetaData>()
                };
                //save

                _radetUploadDAO.Save(upload);
                _radetUploadDAO.CommitChanges();


                uploadError = new UploadLogError
                {
                    RadetUpload = upload,
                    ErrorDetails = new List<ErrorDetails>()
                };


                bool status = false;

                string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/RADET/" + IP.ShortName) + "\\";
                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }


                var filePath = directory + DateTime.Now.ToString("dd MMM yyyy") + "_" + Request.Files[0].FileName;
                //System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/RADET/" + IP.ShortName + "_" + Request.Files[0].FileName);
                Request.Files[0].SaveAs(filePath);

                var validFacilities = Utilities.GetQ4ARTSites(IP.ShortName);

                if (Path.GetExtension(Request.Files[0].FileName).Substring(1).ToUpper() == "ZIP")
                {
                    try
                    {
                        using (ZipFile zipFile = new ZipFile(Request.Files[0].InputStream))
                        {

                            for (int i = 0; i < zipFile.Count; i++)
                            {
                                ZipEntry zipEntry = zipFile[i];
                                string fileName = ZipEntry.CleanName(zipEntry.Name);
                                if (!zipEntry.IsFile)
                                {
                                    continue;
                                }
                                Stream zipStream = zipFile.GetInputStream(zipEntry);
                                MemoryStream stream = new MemoryStream();
                                StreamUtils.Copy(zipStream, stream, new byte[4096]);

                                var radetMetaData = new RadetMetaData(); 

                                status = new RADETProcessor().ReadRadetFile(stream, fileName, IP, LGAs, validFacilities, out radetMetaData, out errors);

                                if (radetMetaData.PatientLineListing == null || radetMetaData.PatientLineListing.Count == 0)
                                {
                                    errors.Add(new ErrorDetails
                                    {
                                        ErrorMessage = "No record found",
                                        FileName = fileName, //Request.Files[0].FileName,
                                        FileTab = "",
                                        LineNo = "",
                                        PatientNo = ""
                                    });
                                }

                                if (errors.Count == 0) //if no errors
                                {
                                    radetMetaData.RadetUpload = upload;
                                    upload.RadetMetaData.Add(radetMetaData);
                                }

                                //what ever status, log feedback to the view via signal R
                                NotifyPage(connectionId, fileName, errors);

                                //append errors if any
                                uploadError.ErrorDetails.AddRange(errors);

                                NotifyPage(((double)(i + 1) / (double)zipFile.Count) * 100, connectionId);
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.LogError(exp);
                        msg = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        msg.Content = new StringContent(exp.Message);
                    }
                }
                else if (Path.GetExtension(Request.Files[0].FileName).Substring(1).ToUpper() == "XLSX")
                {
                    Stream uploadedFileStream = Request.Files[0].InputStream;
                    var radetMetaData = new RadetMetaData(); 

                    status = new RADETProcessor().ReadRadetFile(uploadedFileStream, Request.Files[0].FileName, IP, LGAs, validFacilities, out radetMetaData, out errors);
                    //what ever status, log feedback to the view via signal R
                    NotifyPage(connectionId, Request.Files[0].FileName, errors);

                    if (errors.Count == 0) //if no errors
                    {
                        if (radetMetaData.PatientLineListing == null || radetMetaData.PatientLineListing.Count == 0)
                        {
                            errors.Add(new ErrorDetails
                            {
                                ErrorMessage = "No record found",
                                FileName = Request.Files[0].FileName,
                                FileTab = "",
                                LineNo = "",
                                PatientNo = ""
                            });
                        }
                        else
                        {
                            radetMetaData.RadetUpload = upload;
                            upload.RadetMetaData.Add(radetMetaData);
                        }
                    }

                    //append errors if any
                    uploadError.ErrorDetails.AddRange(errors);
                }
                else
                {
                    msg = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    msg.StatusCode = HttpStatusCode.InternalServerError;
                    msg.Content = new StringContent("invalid file uploaded");
                    return msg;
                }

                msg = new HttpResponseMessage(HttpStatusCode.OK);
            }
            HttpContext.Session[connectionId] = uploadError;

            //notify page for saving record           
            NotifyPage(connectionId);

            RadetUploadErrorLogDAO _logDao = new RadetUploadErrorLogDAO();
            _logDao.Save(uploadError);
            _logDao.CommitChanges();

            bool saved = false;
            RadetMetaDataDAO _radetMetaDataDao = new RadetMetaDataDAO();
            var timespan = _radetMetaDataDao.BulkSave(upload.RadetMetaData.ToList(), out saved);
            stopwatch.Stop();

            if (saved == false && upload.RadetMetaData.ToList().Count > 0)
            {
                //log feedback to the view via signal R
                NotifyPage(connectionId, Request.Files[0].FileName + " _", new List<ErrorDetails>
                {
                    new ErrorDetails
                    {
                        ErrorMessage = "Unable to save data because a previous record was found. If you are re-uploading, please delete the previous upload first and try again.",
                        FileName = Request.Files[0].FileName + " _",
                        FileTab = "",
                        LineNo = "",
                        PatientNo = ""
                    }
                });
            }
            //Notify page of completeness with elapsed time
            //NotifyPage(connectionId, string.Format("Total elapsed time {0}h: {1}m: {2}s", stopwatch.Elapsed.Add(timespan).Hours, stopwatch.Elapsed.Add(timespan).Minutes, stopwatch.Elapsed.Add(timespan).Seconds) + ", " + string.Format("DB operation took {0}h: {1}m: {2}s", timespan.Hours, timespan.Minutes, timespan.Seconds));
            NotifyPage(connectionId, string.Format("Completed. Total elapsed time {0}h: {1}m: {2}s", stopwatch.Elapsed.Add(timespan).Hours, stopwatch.Elapsed.Add(timespan).Minutes, stopwatch.Elapsed.Add(timespan).Seconds));
            return msg;
        }

        public ActionResult downloaderrorsummary(string id)
        {
            string fileName = string.Format("{0:dd-MM-yyyy hh.mm.ss. tt}.csv", DateTime.Now);
            StringBuilder sb = new StringBuilder();
            sb.Append("File Name,");
            sb.Append("Tab Name,");
            sb.Append("Error Message,");
            sb.Append("Line No,");
            sb.Append("Patient Id,");

            sb.AppendLine();

            UploadLogError model = HttpContext.Session[id] as UploadLogError;

            foreach (var m in model.ErrorDetails)
            {
                sb.Append(string.Format("\"{0}\",", m.FileName));
                sb.Append(string.Format("\"{0}\",", m.FileTab));
                sb.Append(string.Format("\"{0}\",", m.ErrorMessage.Replace("<span style='color:red'>", "").Replace("</span>", "")));
                sb.Append(string.Format("\"{0}\",", m.LineNo));
                sb.Append(string.Format("\"{0}\",", m.PatientNo));
                sb.AppendLine();
            }

            System.IO.File.WriteAllText(Server.MapPath("~/downloads/" + fileName), sb.ToString());
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        private void NotifyPage(string connectionId, string fileName, List<ErrorDetails> errorMessagedata)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RadetHub>();

            context.Clients.All.UpdatePages(connectionId, fileName, errorMessagedata);
        }

        private void NotifyPage(double percent, string connectionId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RadetHub>();

            context.Clients.All.Percent(connectionId, percent + "%");
        }

        private void NotifyPage(string connectionId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RadetHub>();
            context.Clients.All.SaveNote(connectionId);
        }

        private void NotifyPage(string connectionId, string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RadetHub>();
            if (message == "validation")
            {
                context.Clients.All.ValidateNote(connectionId);
            }
            else
            {
                context.Clients.All.CompletionNote(connectionId, message);
            }
        }


    }


    public class RadetHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }
    }

    public class UploadModel
    {
        public string connectionId { get; set; }
        public int ip { get; set; }
    }
}
