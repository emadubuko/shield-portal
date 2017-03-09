using DQA.DAL.Business;
using DQA.DAL.Model;
using DQA.ViewModel;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ShieldPortal.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;


namespace ShieldPortal.Controllers
{
   
    public class DQAApiController : ApiController
    {
        readonly MetaDataService metadataService = new MetaDataService();
        // GET: api/DQA
      
        // POST: api/DQA
        public string Post()
        {
            var messages = "";
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    string ext = Path.GetExtension(postedFile.FileName).Substring(1);

                    if (ext.ToUpper() == "XLS" || ext.ToUpper() == "XLSX" || ext.ToUpper() == "XLSM" || ext.ToUpper() == "ZIP")
                    {

                        var filePath = HttpContext.Current.Server.MapPath("~/Report/Uploads/" + postedFile.FileName);
                        postedFile.SaveAs(filePath);
                        

                        if (ext.ToUpper() == "ZIP")
                        {
                            messages+= "<tr><td class='text-center'><i class='icon-check icon-larger green-color'></i></td><td><strong>" + postedFile.FileName + "</strong> : Decompressing please wait.</td></tr>";
                            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            using (ZipFile zipFile = new ZipFile(fs))
                            {
                                var countProcessed = 0;
                                var countFailed = 0;
                                var countSuccess = 0;
                                var step = 0;
                                var total = (int)zipFile.Count;
                                var currentFile = "";

                                foreach (ZipEntry zipEntry in zipFile)
                                {
                                    step++;
                                    //Thread.Sleep(10000);


                                    try
                                    {
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
                                            using (FileStream entryStream = File.Create(extractedFilePath))
                                            {
                                                StreamUtils.Copy(zipStream, entryStream, new byte[4096]);
                                            }
                                            //BLoadWorkbookData wkb = new BLoadWorkbookData(this._ReportType);
                                            messages += new BDQA().ReadWorkbook(extractedFilePath, User.Identity.Name);
                                            //wkb.ProcessWorkbookData(extractedFilePath, this.Weeks, this.Year, this.Months, thread);
                                            countSuccess++;
                                        }
                                    }
                                    catch (Exception exp)
                                    {
                                        countFailed++;
                                        //SRDLog.WriteToLog(wkb.EventId, currentFile + "|" + exp.Message, "", EventTypes.Upload, EventSeverity.Failure);
                                        messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td><strong>" + postedFile.FileName + "</strong>: An Error occured. Please check the files.</td></tr>";
                                    }
                                }
                                zipFile.IsStreamOwner = true;
                                zipFile.Close();

                            }
                        }
                        else
                        {
                            messages += new BDQA().ReadWorkbook(filePath, User.Identity.Name);
                        }
                    }
                    else
                    {
                        messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td><strong>" + postedFile.FileName + "</strong> could not be processed. File is not an excel spreadsheet</td></tr>";
                    }
                }
               // result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return messages;
        }

        public HttpResponseMessage GetTestFile()
        {
            HttpResponseMessage result = null;
            var localFilePath = HttpContext.Current.Server.MapPath("~/timetable.jpg");

            if (!File.Exists(localFilePath))
            {
                result = Request.CreateResponse(HttpStatusCode.Gone);
            }
            else
            {
                // Serve the file to the client
                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "SampleImg";
            }

            return result;
        }

        public List<ReportMetadata> GetIpDQA(int id)
        {
            var metas = metadataService.GetIpMetaData(id);
            var facilities = new List<Facility>();
            var metadatas = new List<ReportMetadata>();
            foreach (var metadata in metas)
            {
                //facilities.Add(new Facility(Utility.GetFacility(metadata.SiteId)))
                metadatas.Add(new ReportMetadata(metadata));
            }


            return metadatas;
        }

        [HttpPost]
        public List<ReportMetadata> SearchIPDQA(int ip,string lga,string state,string facility,string period)
        {
            var metas = metadataService.SearchIpMetaData(ip, period, lga, state, facility);
            var facilities = new List<Facility>();
            var metadatas = new List<ReportMetadata>();
            foreach (var metadata in metas)
            {
                //facilities.Add(new Facility(Utility.GetFacility(metadata.SiteId)))
                metadatas.Add(new ReportMetadata(metadata));
            }


            return metadatas;
        }

        public DataSet GetDQAReport(int id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_dqa_get_facility_quarterly_report";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@metadataId", id);

            return Utility.GetDataSet(cmd);
        }

        public String GenerateReports()
        {
            var metas = new MetaDataService().GetAllMetadata();
            foreach(var meta in metas)
            {
                new BDQA().LoadWorkbook(meta.Id);
            }
            return "Complete";
        }

        public DataSet GetSummaryResult()
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_get_dqa_summary_result";
            cmd.CommandType = CommandType.StoredProcedure;

            return Utility.GetDataSet(cmd);
        }

        public DataSet GetIpSummaryResult(int id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_get_ip_dqa_summary_result";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip", id);

            return Utility.GetDataSet(cmd);
        }

        // DELETE: api/DQA/5
        public void Delete(int id)
        {
            new BDQA().Delete(id);
        }
    }
}

