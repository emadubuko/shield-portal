using System;
using System.Web.Mvc;
using BWReport.DAL.DAO;
using BWReport.DAL.Services;
using System.Net;
using System.IO;
using DMP.ViewModel.BWR;
using System.Collections.Generic;
using BWReport.DAL.Entities;

namespace DMP.Controllers
{
    public class BiWeeklyReportController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            ReportViewModel reportList = new ReportViewModel
            {
                Reports = new ReportUploadsDao().RetrieveAll()
            };

            return View(reportList);
        }

        [HttpPost]
        public ActionResult Upload(DateTime reportingPeriodFrom, DateTime reportingPeriodTo)
        {
            var files = Request.Files;
            if(files == null  || files.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "no files uploaded");
            }
            string fileName = files[0].FileName;
            string fullfilename = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/_" + fileName);
             
            Stream fileContent = files[0].InputStream;
            string loggedInUser = "Admin";
            try
            {
                bool result = new ReportLoader().ExtractReport(reportingPeriodFrom, reportingPeriodTo, fullfilename, fileName, fileContent, loggedInUser);
                return new HttpStatusCodeResult(HttpStatusCode.OK); // Ok();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<GoogleLocation> GetLocationAsync(GPSDataModel location)
        //{
        //    string key = "AIzaSyBYzNzHAY8dZ0mrPeWEss7feDZ6hHdRa4I";
        //    HttpClient http = new HttpClient();
        //    string address = "";

        //    if (location != null)
        //    {
        //        address = location.location;
        //        if (!location.location.ToLower().EndsWith(" nig"))
        //        {
        //            address = location.location + "nigeria";
        //        }
        //        if (!location.location.ToLower().Contains("nigeria"))
        //        {
        //            address = location.location + "nigeria";
        //        }
        //    }
        //    var response = await http.GetAsync("https://maps.googleapis.com/maps/api/geocode/json?sensor=false&address=" + location.location + "&key=" + key);

        //    string result = response.Content.ReadAsStringAsync().Result;
        //    if (result.Contains(")"))
        //    {
        //        string[] split = result.Split('(', ')');
        //        if (split.Count() > 1)
        //            result = split[1];
        //    }
        //    var responseResult = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleAPIResponse>(result);
        //    return responseResult.status.ToUpper() == "OK" ? responseResult.results[0].geometry.location : null;
        //}



    }
}