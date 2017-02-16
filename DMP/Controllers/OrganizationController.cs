using CommonUtil.DAO;
using CommonUtil.Entities;
using DAL.DAO;
using DAL.Entities;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize(Roles ="sys_admin,shield_team")]
    public class OrganizationController : Controller
    {
        // GET: Organization
        public ActionResult Index()
        {
            var organizationList = new OrganizationDAO().RetrieveAll();
            return View(organizationList);
        }

        public ActionResult OrganizationDetail(int OrgId)
        {
            var orgDetail = new OrganizationDAO().Retrieve(OrgId);
            return View(orgDetail);
        }

        
        public ActionResult OrganizationEdit(int OrgId)
        {
            Organizations previousData = new OrganizationDAO().Retrieve(OrgId);
            HttpContext.Session.Add(":previousData:", previousData);
            return View(previousData);
        }

        public ActionResult EditOrganization(Organizations org)
        {
            if (org == null || org.Id== 0)
            {
                return new HttpStatusCodeResult(400, "invalid update");
            }
            Organizations previousData = HttpContext.Session[":previousData:"] as Organizations;
            if (previousData == null || previousData.Id !=org.Id)
            {
                return new HttpStatusCodeResult(400, "invalid update");  
            }

            if (org.Logo == null)
            {
                org.Logo = previousData.Logo;
            }
            var orgDao = new OrganizationDAO();
            orgDao.Update(org);
            orgDao.CommitChanges();

            //update the global cache for organizations
            HttpContext.Session["OrganizationList"] = orgDao.RetrieveAll();

            return RedirectToAction("Index");
        }

        public ActionResult CreateOrganization()
        {
            var vM = new CreateIPViewModel();
            return View(vM);
        }

        public ActionResult CreateNewOrganization(Organizations Orgz)
        {
            if (Orgz == null)
            {
                return Json("invalid update");
            }
            var orgDao = new OrganizationDAO();
            orgDao.Save(Orgz);
            orgDao.CommitChanges();
            return RedirectToAction("Index");
        }
    }
}