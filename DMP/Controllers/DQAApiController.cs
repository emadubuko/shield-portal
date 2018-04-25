using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Business;
using DQA.DAL.Model;
using DQA.ViewModel;
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
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;


namespace ShieldPortal.Controllers
{

    public class DQAApiController : ApiController
    {
        readonly MetaDataService metadataService = new MetaDataService();
         
        // POST: api/DQA
        public string Post()
        {
            var messages = "";
            try
            {
                Logger.LogInfo(" DQAAPi,post", "processing dqa upload");

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

                            //get the datim file containing the DQA numbers for all the facilities
                            string DatimFileSource = HostingEnvironment.MapPath("~/Report/Template/DatimSource.csv");
                            string[] datimNumbersRaw = File.ReadAllText(DatimFileSource).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

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
                                                using (FileStream entryStream = File.Create(extractedFilePath))
                                                {
                                                    StreamUtils.Copy(zipStream, entryStream, new byte[4096]);
                                                }
                                                messages += new BDQA().ReadWorkbook(extractedFilePath, User.Identity.Name, datimNumbersRaw);
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
                                messages += new BDQA().ReadWorkbook(filePath, User.Identity.Name, datimNumbersRaw);
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
                    result = Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                messages += "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td>System error has occurred</td></tr>";
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
        public List<ReportMetadata> SearchIPDQA(int ip, string lga, string state, string facility, string period)
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

        [HttpPost]
        public HttpResponseMessage GenerateReports()
        {
            string DatimFileSource = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DatimSource.csv");
            string[] datimNumbersRaw = File.ReadAllText(DatimFileSource).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("facility name,facility code, datim_htc_tst, htc_tst, datim_htc_tst_pos, htc_tst_pos, datim_htc_only, htc_only, datim_htc_pos, htc_pos, datim_pmtct_stat, pmtct_stat, datim_pmtct_stat_pos, pmtct_stat_pos, datim_pmtct_stat_previously, pmtct_stat_previoulsy_known, datim_pmtct_eid,pmtct_eid,datim_pmtct_art,pmtct_art,datim_tx_new,tx_new,datim_tx_curr,tx_curr");

            var metas = new MetaDataService().GetAllMetadata();
            foreach (var meta in metas)
            {
                string output = new BDQA().LoadWorkbook(meta.Id, datimNumbersRaw);
                sb.AppendLine(output);
            }

            string DQAComparisonPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Downloads/DQAComparison.csv");
            File.WriteAllText(DQAComparisonPath, sb.ToString());
            HttpResponseMessage result = null;
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(new FileStream(DQAComparisonPath, FileMode.Open, FileAccess.Read));
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "DQAComparison.csv";

            return result;
        }


        [HttpPost]
        public string GenerateDQADimensions()
        {
            string baseFolder = HostingEnvironment.MapPath("~/Report/Downloads/DQAResult/");

            baseFolder = baseFolder + string.Format("{0:MMddhhmmss}", DateTime.Now);
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            string dimension_outputfile = baseFolder + "/DQADimensions.csv";
            string comparsion_outputfile = baseFolder + "/DQAComparsion.csv";


            new BDQA().GenerateDQADimensionsFromDB(dimension_outputfile, comparsion_outputfile);
            // new BDQA().GenerateDQADimensionsFromFile(outputfile);

            string outputZip = baseFolder + ".zip";
            // zip up the files
            try
            {
                using (ZipOutputStream s = new ZipOutputStream(File.Create(outputZip)))
                {
                    s.SetLevel(9); // 0-9, 9 being the highest compression

                    byte[] buffer = new byte[4096];
                    string[] filenames = Directory.GetFiles(baseFolder);
                    foreach (string file in filenames)
                    {
                        ZipEntry entry = new
                        ZipEntry(Path.GetFileName(file));

                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0,
                                buffer.Length);

                                s.Write(buffer, 0, sourceBytes);

                            } while (sourceBytes > 0);
                        }
                    }
                    s.Finish();
                    s.Close();
                }
            }
            catch (Exception ex) { throw ex; }
            string[] output = outputZip.Split(new string[] { "\\", "//" }, StringSplitOptions.None);
            return output[output.Count() - 1];
        }


        public DataSet GetSummaryResult(int year, string Quarter)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_get_dqa_summary_result";
            cmd.Parameters.AddWithValue("@period", Quarter);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.CommandType = CommandType.StoredProcedure;
            return Utility.GetDataSet(cmd);
        }

        public DataSet GetIpSummaryResult(int year, string Quarter, int id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_get_ip_dqa_summary_result";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip", id);
            cmd.Parameters.AddWithValue("@year", year);
            cmd.Parameters.AddWithValue("@period", Quarter);

            return Utility.GetDataSet(cmd);
        }

        // DELETE: api/DQA/5
        [HttpPost]
        public void Delete(int id)
        {
            new BDQA().Delete(id);
        }

        [HttpPost]
        public HttpResponseMessage ProcesssPivotTable(string selectedQuater, int selectedYear)
        {
            HttpResponseMessage msg = null;
            if (HttpContext.Current.Request.Files.Count == 0 || string.IsNullOrEmpty(HttpContext.Current.Request.Files[0].FileName))
            {
                msg = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No file was uploaded");
            }
            else
            {
                string result = "";
                Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
                Stream uploadedFile = HttpContext.Current.Request.Files[0].InputStream;
                bool status = new BDQAQ2().ReadPivotTable(uploadedFile, selectedQuater, selectedYear, loggedinProfile, out result);
                if (status)
                {
                    msg = Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                    msg = Request.CreateErrorResponse(HttpStatusCode.BadRequest, result);
            }
            return msg;
        }


        public IHttpActionResult GetPivotTable([FromUri] string Quater,string IPstring="")
        {
           
            List<string> IPs = JsonConvert.DeserializeObject<List<string>>(IPstring);
            var data = Utility.RetrievePivotTablesForNDR(IPs, Quater); 

            return Ok(data);
        }

        [HttpGet]
        public string GetDashboardStatistic(string period)
        {
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }
            var ds = Utility.GetDashboardStatistic(ip, period);
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

       

        [HttpPost]
        public IHttpActionResult DeletePivotTable(int id)
        {
            Utility.DeletePivotTable(id);
            return Ok();
        }

        /*
        [HttpPost]
        public HttpResponseMessage RetrieveRadet(int id)
        {
            HttpResponseMessage msg = null;
            string result = new RadetUploadReportDAO().RetrieveRadet(id);
            msg = Request.CreateResponse(HttpStatusCode.OK, result);
            return msg;
        }

        [HttpPost]
        public HttpResponseMessage ProcesssRadetFile(string selectedQuater, int selectedYear)
        {
            HttpResponseMessage msg = null;
            if (HttpContext.Current.Request.Files.Count == 0 || string.IsNullOrEmpty(HttpContext.Current.Request.Files[0].FileName))
            {
                msg = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No file was uploaded");
            }
            else
            {
                string result = "";
                Profile loggedinProfile = new Services.Utils().GetloggedInProfile();
                var Ips = new OrganizationDAO().RetrieveAll().ToDictionary(s => s.ShortName.ToLower());
                bool status = false;

                if (Path.GetExtension(HttpContext.Current.Request.Files[0].FileName).Substring(1).ToUpper() == "ZIP")
                {
                    try
                    {
                        using (ZipFile zipFile = new ZipFile(HttpContext.Current.Request.Files[0].InputStream))
                        {
                            for(int i=0; i< zipFile.Count;i++ )
                           // foreach (ZipEntry zipEntry in zipFile)
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
                                try
                                {
                                    status = new RadetUploadReportDAO().ReadRadetFile(stream, selectedQuater, selectedYear, loggedinProfile, Ips, fileName ,true, out result);
                                }
                                catch (Exception ex)
                                {
                                    result += " (" + zipEntry.Name + ")";
                                    Logger.LogError(ex);
                                    break;
                                }
                                if (status == false && result == "Result already exist")
                                    continue;
                                else if (status == false)
                                {
                                    result += " (" + zipEntry.Name + ")";
                                    break;
                                }
                                    
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
                    Stream uploadedFile = HttpContext.Current.Request.Files[0].InputStream;
                    status = new RadetUploadReportDAO().ReadRadetFile(uploadedFile, selectedQuater, selectedYear, loggedinProfile, Ips, null, false, out result);
                }

                if (status)
                {
                    msg = Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                    msg = Request.CreateErrorResponse(HttpStatusCode.BadRequest, result);
            }
            return msg;
        }

        [HttpPost]
        public IHttpActionResult DeleteRadetFile(int id)
        {
            RadetUploadReportDAO dao = new RadetUploadReportDAO();
            try
            {
                dao.DeleteRecord(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }

        */
    }
}

