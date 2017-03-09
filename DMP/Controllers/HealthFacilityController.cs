using CommonUtil.DAO;
using CommonUtil.Entities;
using ShieldPortal.ViewModel.BWR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class HealthFacilityController : Controller
    {
        public ActionResult Index()
        {
            var facilitiest = new HealthFacilityDAO().RetrieveAll();
            return View(facilitiest);
        }

        [HttpPost]
        public ActionResult Upload()
        {
            var files = Request.Files;
            if (files == null || files.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "no files uploaded");
            }

            string[] theLines;
            using (StreamReader reader = new StreamReader(files[0].InputStream))
            {
                reader.ReadLine();
                string filecontent = reader.ReadToEnd();
                theLines = filecontent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            //Todo: finish this upload later
            string response = new HealthFacilityDAO().SaveFromCSV(theLines);

            if (string.IsNullOrEmpty(response))
                return Json("Successful");
            else
                return Json("An error occurred");
        }

        public ActionResult HealthFacilityDetail(int facilityId)
        {
            var facilityDetail = new HealthFacilityDAO().Retrieve(facilityId);
            return View(facilityDetail);
        }


        public ActionResult HealthFacilityEdit(int facilityId)
        {
            HealthFacility previousData = new HealthFacilityDAO().Retrieve(facilityId);
            HttpContext.Session.Add(":previousData:", previousData);
            return View(previousData);
        }

        public ActionResult EditHealthFacility(HealthFacility facility)
        {
            if (facility == null || facility.Id == 0)
            {
                return new HttpStatusCodeResult(400, "invalid update");
            }
            HealthFacility previousData = HttpContext.Session[":previousData:"] as HealthFacility;
            if (previousData == null || previousData.Id != facility.Id)
            {
                return new HttpStatusCodeResult(400, "invalid update");
            }

             
            var facilityDao = new HealthFacilityDAO();
            facilityDao.Update(facility);
            facilityDao.CommitChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CreateHealthFacility()
        {
            var vM = new HealthFacilityViewModel();
            vM.LGA = new LGADao().RetrieveAll() as List<LGA>;
            vM.Organizations = new OrganizationDAO().RetrieveAll() as List<Organizations>;
            return View(vM);
        }

        public ActionResult CreateNewHealthFacility(HealthFacility facility)
        {
            if (facility == null)
            {
                return Json("invalid update");
            }
            var facilityDao = new HealthFacilityDAO();
            facility.LGA = new LGADao().Retrieve(facility.lgacode);

            facilityDao.Save(facility);
            facilityDao.CommitChanges();
            return RedirectToAction("Index");
        }
    }
}