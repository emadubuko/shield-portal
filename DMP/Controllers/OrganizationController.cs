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

        public ActionResult CreateOrganization()
        {
            var vM = new CreateIPViewModel();
            return View(vM);
        }

        public ActionResult CreateNewOrganization(Organizations Orgz)
        {
            if(Orgz != null)
            {
                SaveOrUpdate(Orgz);

                return RedirectToAction("Index");
            }
            return Json("Ok");
        }

        public void SaveOrUpdate(Organizations Orgz)
        {
            var orgDao = new OrganizationDAO();
            if(Orgz.Id == 0)
            {
                orgDao.Save(Orgz);
            }else
            {
                orgDao.Update(Orgz);
            }
            
            orgDao.CommitChanges();
        }
    }
}