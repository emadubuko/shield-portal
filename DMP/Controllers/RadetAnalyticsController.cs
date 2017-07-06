using CommonUtil.Entities;
using CommonUtil.Utilities;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNet.SignalR;
using RADET.DAL.DAO;
using RADET.DAL.Entities;
using RADET.DAL.Services;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [System.Web.Mvc.Authorize]
    public class RadetAnalyticsController : Controller
    {      
        public ActionResult Index()
        {
            return View();
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
            List<RadetReportModel2> list = GetPreviousUpload("Q2 FY17");
            return View(list);
        }

        public ActionResult ViewPreviousUploads()
        {  
            List<RadetReportModel2> list = GetPreviousUpload("Q2 FY17");
            return View(list);
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

        public List<RadetReportModel2> GetPreviousUpload(string RadetPeriod, int IP=0)
        {
            var profile = new Services.Utils().GetloggedInProfile();
            RadetMetaDataDAO metaDao = new RadetMetaDataDAO();

            IQueryable<RadetMetaData> previousUploads = null;

            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = metaDao.RetrieveRadetUpload("Q2 FY17", IP);
            }
            else
            {
                previousUploads = metaDao.RetrieveRadetUpload("Q2 FY17", profile.Organization.Id);
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
                            Id = entry.Id,
                        }).ToList();
            }
            return list;
        }

        [HttpPost]
        public JsonResult RetrieveRadet(int id)
        {
            string result = new RadetMetaDataDAO().RetrieveRadetData(id);
            return Json(result);
        }

        public JsonResult RandomizeRadet(int id, int active, int inactive)
        {
           var result =   new RADETProcessor().Randomizetems(id, active, inactive);
            return Json(string.Format("{0} active patients selected out {1}; <br /> {2} inactive patients out of {3} <br /> Close this dialog and click on view list button to view randoized result", result.Count(x=>x.CurrentARTStatus == "Active" && x.SelectedForDQA), result.Count(x=>x.CurrentARTStatus == "Active"), result.Count(x=>x.CurrentARTStatus !="Active" && x.SelectedForDQA), result.Count(x=>x.CurrentARTStatus !="Active")));
        }

        [HttpPost]
        public HttpResponseMessage RadetFileUpload(string connectionId, int ip)
        {
            HttpResponseMessage msg = null;
            UploadLogError uploadError = null;
            RadetUpload upload = null;
            var stopwatch = new Stopwatch();

            RadetUploadDAO _radetUploadDAO = new RadetUploadDAO();
             
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
                                var errors = new List<ErrorDetails>();

                                status = new RADETProcessor().ReadRadetFile(stream, fileName, IP, LGAs, out radetMetaData, out errors);

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
                else if(Path.GetExtension(Request.Files[0].FileName).Substring(1).ToUpper() == "XLSX")
                {
                    Stream uploadedFileStream = Request.Files[0].InputStream;
                    var radetMetaData = new RadetMetaData();
                    var errors = new List<ErrorDetails>();

                    status = new RADETProcessor().ReadRadetFile(uploadedFileStream, Request.Files[0].FileName, IP, LGAs, out radetMetaData, out errors);
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
             
            RadetMetaDataDAO _radetMetaDataDao = new RadetMetaDataDAO();
            var timespan = _radetMetaDataDao.BulkSave(upload.RadetMetaData.ToList()); 
            stopwatch.Stop();


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
            if(message == "validation")
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
