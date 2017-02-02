using DQA.DAL.Business;
using DQA.DAL.Model;
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
   // [EnableCors(origins: "http://localhost:65097", headers: "*", methods: "*")]
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

                    if (ext.ToUpper() == "XLS" || ext.ToUpper() == "XLSX")
                    {
                        
                        var filePath = HttpContext.Current.Server.MapPath("~/Report/Uploads" + postedFile.FileName);
                        postedFile.SaveAs(filePath);
                        new BDQA().ReadWorkbook(filePath);
                    }
                    else
                    {
                        messages+= "<tr><td class='text-center'><i class='icon-cancel icon-larger red-color'></i></td><td><strong>" + postedFile.FileName + "</strong> could not be processed. File is not an excel spreadsheet</td></tr>";
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
            var metadatas = new List<ReportMetadata>();
            foreach (var metadata in metas)
            {
                metadatas.Add(new ReportMetadata(metadata));
            }
            return metadatas;
        }

        public DataTable GetDQAReport(int id)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_dqa_get_facility_quarterly_report";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@metadataId", id);

            return Utility.GetDatable(cmd);
        }

      

        // DELETE: api/DQA/5
        public void Delete(int id)
        {
            new BDQA().Delete(id);
        }
    }
}

