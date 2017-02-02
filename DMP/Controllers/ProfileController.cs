using CommonUtil.DAO;
using CommonUtil.Entities;
using DAL.DAO;
using DAL.Entities;
using System;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        [Authorize(Roles = "sys_admin,shield_team")]
        public ActionResult Index()
        {
            var profiles = new ProfileDAO().RetrieveAll();
            return View(profiles);
        }

        public ActionResult CreateProfile()
        {
            return RedirectToAction("Register", "Account");
            //return View(new ProfileViewModel());
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
            var orgs = new OrganizationDAO().RetrieveAll();
            ViewBag.Organizations = orgs;
            ViewBag.Roles = Services.Utils.RetrieveRoles();
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

            var previous = profileDao.Retrieve(pGuid);
            if (System.Web.HttpContext.Current.User.IsInRole("sys_admin"))
            {
                Services.Utils.UpdateUserToRole(profile.Username, previous.RoleName, profile.RoleName);
                profile.Organization = new OrganizationDAO().Retrieve(profile.OrganizationId);
            }
            else
            {
                profile.Organization = previous.Organization;
            }
            profileDao.Update(profile);
            profileDao.CommitChanges();

            if (System.Web.HttpContext.Current.User.IsInRole("sys_admin"))
            {
                return RedirectToAction("Index");
            }
            else
            {
               return RedirectToAction("Index", "Home");
            }  
        }


        //public ActionResult CreateNewProfile(Profile profile)
        //{
        //    if (profile != null)
        //    {
        //        var profileDAO = new ProfileDAO();
        //        profileDAO.Save(profile);
        //        profileDAO.CommitChanges();                

        //        return RedirectToAction("Index");
        //    }
        //    return Json("Ok");
        //}
 
    }
}