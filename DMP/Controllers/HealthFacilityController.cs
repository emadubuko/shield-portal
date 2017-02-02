using BWReport.DAL.DAO;
using BWReport.DAL.Entities;
using CommonUtil.DAO;
using CommonUtil.Entities;
using ShieldPortal.ViewModel;
using ShieldPortal.ViewModel.BWR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
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
            string filecontent = "";

            using (StreamReader reader = new StreamReader(files[0].InputStream))
            {
                filecontent = reader.ReadToEnd();
            }
            //Todo: finish this upload later


            return Json("Successful");
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