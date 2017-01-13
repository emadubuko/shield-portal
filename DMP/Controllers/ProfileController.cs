using DAL.DAO;
using DAL.Entities;
using DMP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DMP.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            var profiles = new ProfileDAO().RetrieveAll();
            return View(profiles);
        }

        public ActionResult CreateProfile()
        {
            return View(new ProfileViewModel());
        }

        public ActionResult CreateNewProfile(Profile profile)
        {
            if (profile != null)
            {
                SaveOrUpdate(profile);

                return RedirectToAction("Index");
            }
            return Json("Ok");
        }

        public void SaveOrUpdate(Profile profile)
        {
            var orgDao = new ProfileDAO();
            //if (profile.Id == null)
            {
                orgDao.Save(profile);
            }
            //else
            //{
            //   // orgDao.Update(profile);
            //}

            orgDao.CommitChanges();
        }
    }
}