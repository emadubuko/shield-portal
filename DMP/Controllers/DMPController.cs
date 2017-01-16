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
    public class DMPController : Controller
    {
        OrganizationDAO orgDAO = null;
        DMPDAO dmpDAO = null;
        static Guid guid = new Guid("CC16C80A-593F-4AB5-837C-A6F301107842");
        static Profile initiator = new ProfileDAO().Retrieve(guid);

        public DMPController()
        {
            dmpDAO = new DMPDAO();
            orgDAO = new OrganizationDAO();
        }
        // GET: DMP
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateDMP()
        {
            CreateDMPViewModel vM = new CreateDMPViewModel();
            vM.OrganizationList = OrgsRepo();
            return View(vM);
        }


        [HttpPost]
        public ActionResult SaveDMP(DAL.Entities.DMP newDMP)
        {
            if (newDMP == null || string.IsNullOrEmpty(newDMP.DMPTitle))
            {
                return new HttpStatusCodeResult(400, "Please provide a title");
            }
            DAL.Entities.DMP MyDMP = newDMP;
            MyDMP.DateCreated = DateTime.Now;
            MyDMP.CreatedBy = initiator;
            MyDMP.Organization = OrgsRepo().FirstOrDefault(x=>x.Id == newDMP.OrganizationId);


            var tmpDMP = dmpDAO.SearchByName(MyDMP.DMPTitle);
            if (tmpDMP == null)
            {
                dmpDAO.Save(MyDMP);
                dmpDAO.CommitChanges();
                return Json(MyDMP.Id);
            }
            else
            {
                return new HttpStatusCodeResult(400, "DMP with this title already exists");
            }           
        }

        private IList<Organizations> OrgsRepo()
        {
            if ((HttpContext.Session["OrganizationList"] as List<Organizations>) == null)
            {
                HttpContext.Session["OrganizationList"] = orgDAO.RetrieveAll();
            }
            return HttpContext.Session["OrganizationList"] as List<Organizations>;
        }
    }
}