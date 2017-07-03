using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNet.SignalR;
using RADET.DAL.Entities;
using RADET.DAL.Services;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
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
        // GET: RadetAnalytics
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadPage()
        {
            var profile = new Services.Utils().GetloggedInProfile();

            List<RadetUploadReport> previousUploads = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                //previousUploads = new RadetUploadReportDAO().RetrieveRadetUpload(0, 2017, "Q2(Jan-Mar)");
                ViewBag.showdelete = true;
                ViewBag.Orgs = new OrganizationDAO().RetrieveAll().Where(x => x.ShortName != "MGIC" && x.ShortName != "CDC");
            }
            else
            {
                ViewBag.Orgs = new List<Organizations> { profile.Organization };
                ViewBag.showdelete = false;
                //previousUploads = new RadetUploadReportDAO().RetrieveRadetUpload(profile.Organization.Id, 2017, "Q2(Jan-Mar)");
            }
            List<RadetReportModel> list = new List<RadetReportModel>();
            //if (previousUploads != null)
            //{
            //    list = (from entry in previousUploads
            //            select new RadetReportModel
            //            {
            //                Facility = entry.Facility,
            //                IP = entry.IP.ShortName,
            //                dqa_quarter = entry.dqa_quarter,
            //                dqa_year = entry.dqa_year,
            //                UploadedBy = entry.UploadedBy.FullName,
            //                DateUploaded = entry.DateUploaded,
            //                Id = entry.Id,

            //            }).ToList();
            //}

            return View(list);
        }



        [HttpPost]
        public HttpResponseMessage RadetFileUpload(string connectionId, int ip)
        {

            HttpResponseMessage msg = null;
            UploadLogError uploadError = null;

            if (Request.Files.Count == 0 || string.IsNullOrEmpty(Request.Files[0].FileName))
            {
                msg = new HttpResponseMessage(HttpStatusCode.BadRequest);
                msg.Content = new StringContent("No file uploaded");

                NotifyPage(connectionId, "no file uploaded",
                    new List<ErrorDetails> { new ErrorDetails {
                    FileName = "no file uploaded", ErrorMessage = "no file uploaded", FileTab="", LineNo="", PatientNo=""
                }});

                return msg;
            }
            else
            {
                bool result = false;
                Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
                var IP = new OrganizationDAO().Retrieve(ip);
                if (IP == null)
                {
                    msg = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    msg.Content = new StringContent("invalid IP");

                    NotifyPage(connectionId, "invalid IP selected",
                    new List<ErrorDetails> { new ErrorDetails {
                            FileName = "invalid IP selected", ErrorMessage = "invalid IP selected", FileTab="", LineNo="", PatientNo=""
                        }});

                    return msg;
                }

                var LGAs = new LGADao().RetrieveAll();


                var errorDetails = new List<ErrorDetails>();

                RadetUpload upload = new RadetUpload
                {
                    DateUploaded = DateTime.Now,
                    IP = IP,
                    UploadedBy = loggedinProfile,
                    RadetMetaData = new List<RadetMetaData>()
                };
                //save
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
                                //what ever status, log feedback to the view via signal R
                                NotifyPage(connectionId, fileName, errors);

                                //append errors if any
                                uploadError.ErrorDetails.AddRange(errors);

                                if (errors.Count == 0) //if no errors
                                {
                                    upload.RadetMetaData.Add(radetMetaData);
                                    radetMetaData.RadetUpload = upload;
                                }

                                NotifyPage(((double)(i + 1) / (double)zipFile.Count) * 100, connectionId);
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.LogError(exp);
                    }
                }
                else
                {
                    Stream uploadedFileStream = Request.Files[0].InputStream;
                    var radetMetaData = new RadetMetaData();
                    var errors = new List<ErrorDetails>();

                    status = new RADETProcessor().ReadRadetFile(uploadedFileStream, Request.Files[0].FileName, IP, LGAs, out radetMetaData, out errors);
                    if (status)
                    {
                        upload.RadetMetaData.Add(radetMetaData);
                        radetMetaData.RadetUpload = upload;
                    }
                    else
                    {
                        uploadError.ErrorDetails.AddRange(errors);
                    }
                }

                if (status)
                {
                    msg = new HttpResponseMessage(HttpStatusCode.OK);//, result);
                }
                else
                    msg = new HttpResponseMessage(HttpStatusCode.BadRequest);//, result);
            }

            HttpContext.Session[connectionId] = uploadError;
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

            UploadLogError model =  HttpContext.Session[id] as UploadLogError;

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
