using CommonUtil.DAO;
using CommonUtil.Entities;
using DAL.DAO;
using DAL.Entities;
using ShieldPortal.Services;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShieldPortal.ViewModel.DMP;

namespace ShieldPortal.Controllers
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
        // GET: ShieldPortal
        public ActionResult Index()
        {
            bool isIpUser = System.Web.HttpContext.Current.User.IsInRole("ip");
            var currentProfile = new Services.Utils().GetloggedInProfile();

            List<DAL.Entities.DMP> dmps = new List<DAL.Entities.DMP>();
            if (isIpUser)
            {
                dmps = dmpDAO.SearchOrganizaionId(currentProfile.Organization.Id) as List<DAL.Entities.DMP>;
            }
            else
            {
                dmps = dmpDAO.RetrieveDMPSorted() as List<DAL.Entities.DMP>; //.RetrieveAll().Where(x => x.TheProject != null).ToList();
            }

            List<DMPViewModel> dmpVM = new List<DMPViewModel>();
            dmps.ForEach(x =>
            {
                dmpVM.Add(new DMPViewModel
                {
                    Id = x.Id,
                    CreatedBy = x.CreatedBy != null ? x.CreatedBy.FullName : "test",
                    DateCreated = string.Format("{0:dd-MMM-yyy}", x.DateCreated),
                    ProjectTitle = x.TheProject.ProjectTitle,
                    Title = "DMP_" + x.TheProject.ProjectShortName, //x.DMPTitle,
                    Owner = x.Organization.ShortName,
                    StartDate = string.Format("{0:dd-MMM-yyy}", x.StartDate),
                    EndDate = string.Format("{0:dd-MMM-yyy}", x.EndDate)
                    //Status = ((DMPStatus)x.Status).ToString()
                });
            });

            ViewBag.ShowCreateLink = currentProfile.Organization.SubscribedApps !=null && currentProfile.Organization.SubscribedApps.Contains("DMP");
            return View(dmpVM);
        }

        public ActionResult CreateDMP()
        {
            bool isIpUser = System.Web.HttpContext.Current.User.IsInRole("ip");
            CreateDMPViewModel vM = new CreateDMPViewModel();
            
            if (isIpUser)
            {
                var Ip = new Services.Utils().GetloggedInProfile().Organization;
                if (Ip.SubscribedApps !=null && Ip.SubscribedApps.Contains("DMP"))
                {
                    vM.OrganizationList = new List<Organizations> { OrgsRepo().FirstOrDefault(c=>c.Id == Ip.Id) }; 
                }                
            }
            else
            {
                vM.OrganizationList = OrgsRepo().Where(ip=> ip.SubscribedApps != null &&  ip.SubscribedApps.Contains("DMP")).ToList();
            } 
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
            MyDMP.Organization = OrgsRepo().FirstOrDefault(x => x.Id == newDMP.OrganizationId);


            var tmpDMP = dmpDAO.SearchByName(MyDMP.DMPTitle);
            if (tmpDMP == null)
            {
                dmpDAO.Save(MyDMP);
                dmpDAO.CommitChanges();
                return Json(MyDMP.Id);
            }
            else
            {
                return new HttpStatusCodeResult(400, "ShieldPortal with this title already exists");
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

        public ActionResult DMPDetails(int dmpId)
        {
            var currentProfile = new Services.Utils().GetloggedInProfile();
            List<DMPDocumentDetails> dmpDoc = new List<DMPDocumentDetails>();

            var dmpDocuments = dmpDocDAO.SearchByDMP(dmpId).ToList();
            dmpDocuments.ForEach(x =>
                dmpDoc.Add(
                    new DMPDocumentDetails
                    {
                        ApprovedBy = x.ApprovedBy == null ? "" : x.ApprovedBy.FullName,
                        ApprovedDate = string.Format("{0:dd-MMM-yyyy}", x.ApprovedDate),
                        CreationDate = string.Format("{0:dd-MMM-yyyy}", x.CreationDate),
                        DMPId = dmpId,
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
            var latestDoc = dmpDocuments.LastOrDefault();
            DMPDocumentViewModel dmpDocVM = new DMPDocumentViewModel
            {
                DmpDetails = new DMPViewModel
                {
                    DateCreated = string.Format("{0:dd-MMM-yyyy}", latestDoc.CreationDate),
                    CreatedBy = latestDoc.Initiator.FullName,
                    Id = latestDoc.TheDMP.Id,
                    ProjectTitle = latestDoc.TheDMP.TheProject.ProjectTitle,
                    Owner = latestDoc.TheDMP.Organization.ShortName,
                    StartDate = string.Format("{0:dd-MMM-yyyy}", latestDoc.TheDMP.StartDate),
                    EndDate = string.Format("{0:dd-MMM-yyyy}", latestDoc.TheDMP.EndDate),
                    Status = ((DMPStatus)latestDoc.Status).ToString(),
                    Title = latestDoc.DocumentTitle
                },//dmpVM,
                Documents = dmpDoc
            };
            ViewBag.ShowCreateLink = currentProfile.Organization.SubscribedApps != null && currentProfile.Organization.SubscribedApps.Contains("DMP");
            return View(dmpDocVM);
        }

        public ActionResult DMPDashboard()
        {
            if (!Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["allow_view_dmp_dashboard"]))
            {
                return RedirectToAction("index");
            } 

            IList<DMPDocument> dmpDocuments = new DMPDocumentDAO().RetrieveAll()
                .Where(x => x.TheDMP != null && x.TheDMP.TheProject != null)
                .ToList();
            List<StaffStatusByRoles> StaffStatusByRoles = new List<StaffStatusByRoles>();
            foreach (var item in dmpDocuments)
            {
                Organizations Ip = item.TheDMP.Organization;
                List<StaffGrouping> roles = item.Document.MonitoringAndEvaluationSystems.People.Roles;
                foreach (var r in roles)
                {
                    var previous_entry = StaffStatusByRoles.FirstOrDefault(c => c.IP == Ip.ShortName); // r.ip c.RoleName.ToLower().Trim().Contains(r.Name.ToLower().Trim()));
                    if (previous_entry == null)
                    {
                        StaffStatusByRoles.Add(
                            new StaffStatusByRoles
                            {
                                IP = Ip.ShortName,
                                RoleCount = new Dictionary<string, int>
                                { { r.Name.ToLower() == "si officer" ? "M&EO" : r.Name, (r.SiteCount + r.RegionCount + r.HQCount)} }
                            });
                    }
                    else
                    {
                        previous_entry.RoleCount.Add(r.Name.ToLower() == "si officer" ? "M&EO" : r.Name, (r.SiteCount + r.RegionCount + r.HQCount));
                    }
                }
            }

            List<string> role_header = new List<string>();
            foreach (var item in StaffStatusByRoles)
            {
                foreach (var key in item.RoleCount.Keys)
                {
                    if (role_header.FirstOrDefault(x => x == key) == null)
                        role_header.Add(key);
                }
            }
            ViewBag.headers = role_header;

            return View(StaffStatusByRoles);
        }
    }


}