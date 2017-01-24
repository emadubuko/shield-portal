using DAL.DAO;
using DAL.Entities;
using DMP.Services;
using DMP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DMP.Controllers
{
    [Authorize]
    public class DMPController : Controller
    {
        OrganizationDAO orgDAO = null;
        DMPDAO dmpDAO = null;
        DMPDocumentDAO dmpDocDAO = null;
      
        public DMPController()
        {
            dmpDAO = new DMPDAO();
            orgDAO = new OrganizationDAO();
            dmpDocDAO = new DMPDocumentDAO();
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
            MyDMP.CreatedBy = new Utils().GetloggedInProfile();  
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

        public ActionResult DMPDetails(DMPViewModel dmpVM)
        {
            List<DMPDocumentDetails> dmpDoc = new List<DMPDocumentDetails>();

            var dmpDocuments = dmpDocDAO.SearchByDMP(dmpVM.Id).ToList();
            dmpDocuments.ForEach(x =>
                dmpDoc.Add(
                    new DMPDocumentDetails
                    {
                        ApprovedBy = x.ApprovedBy == null ? "" : x.ApprovedBy.FullName,
                        ApprovedDate = string.Format("{0:dd-MMM-yyyy}", x.ApprovedDate),
                        CreationDate = string.Format("{0:dd-MMM-yyyy}", x.CreationDate),
                        DMPId = dmpVM.Id,
                        DocumentCreator = x.Initiator.FullName,
                        DocumentTitle = x.DocumentTitle,
                        DocumentId = x.Id.ToString(),
                        LastModifiedDate = string.Format("{0:dd-MMM-yyyy}", x.LastModifiedDate),
                        ReferralCount = x.ReferralCount,
                        Status = ((DMPStatus)x.Status).ToString(),
                        Version = x.Version,
                        PageNumber = x.PageNumber
                    })
                );
            DMPDocumentViewModel dmpDocVM = new DMPDocumentViewModel
            {
                DmpDetails = dmpVM,
                Documents = dmpDoc
            };

            return View(dmpDocVM);
        }
    }
}