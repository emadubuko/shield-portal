﻿using DAL.DAO;
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

        public ActionResult ProfileDetail(string profileId)
        {
            Guid pGuid = new Guid(profileId);
            var profile = new ProfileDAO().Retrieve(pGuid);
            return View(profile);
        }

        public ActionResult profileEdit(string profileId)
        {
            Guid pGuid = new Guid(profileId);
            var pDetails = new ProfileDAO().Retrieve(pGuid);
            return View(pDetails);
        }

        public ActionResult EditProfile(Profile profile, string profileId)
        {
            if (profile == null || string.IsNullOrEmpty(profileId))
            {
                return new HttpStatusCodeResult(400, "invalid update");
            }
            Guid pGuid = new Guid(profileId);
            profile.Id = pGuid;
            var profileDao = new ProfileDAO();
            profileDao.Update(profile);
            profileDao.CommitChanges();
            return RedirectToAction("Index");
        }


        public ActionResult CreateNewProfile(Profile profile)
        {
            if (profile != null)
            {
                var profileDAO = new ProfileDAO();
                profileDAO.Save(profile);
                profileDAO.CommitChanges();                

                return RedirectToAction("Index");
            }
            return Json("Ok");
        }
 
    }
}