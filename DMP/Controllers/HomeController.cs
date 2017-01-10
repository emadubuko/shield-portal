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
    public class HomeController : Controller
    {
        DMPDAO dmpDAO = null;
        DMPDocumentDAO dmpDocDAO = null;
        OrganizationDAO orgDAO = null;
        ProjectDetailsDAO projDAO = null;

        static DAL.Entities.DMP MyDMP = new DAL.Entities.DMP();

        static Guid guid = new Guid("CC16C80A-593F-4AB5-837C-A6F301107842");
        static Profile initiator = new ProfileDAO().Retrieve(guid);

        public HomeController()
        {
            dmpDAO = new DMPDAO();
            dmpDocDAO = new DMPDocumentDAO();
            orgDAO = new OrganizationDAO();
            projDAO = new ProjectDetailsDAO();

        }

        private IList<Organizations> OrgsRepo()
        {
            if ((HttpContext.Session["OrganizationList"] as List<Organizations>) == null)
            {
                HttpContext.Session["OrganizationList"] = orgDAO.RetrieveAll();
            }
            return HttpContext.Session["OrganizationList"] as List<Organizations>;
        }

        public ActionResult Index()
        {
            var dmps = dmpDAO.RetrieveAll().Where(x => x.TheProject != null).ToList();

            List<DMPViewModel> dmpVM = new List<ViewModel.DMPViewModel>();
            dmps.ForEach(x =>
            {
                dmpVM.Add(new DMPViewModel
                {
                    Id = x.Id,
                    CreatedBy = x.CreatedBy != null ? x.CreatedBy.FullName : "test",
                    DateCreated = string.Format("{0:dd-MMM-yyy}", x.DateCreated),
                    ProjectTitle = x.TheProject.ProjectTitle,
                    Title = x.DMPTitle,
                    Owner = x.Organization.ShortName,
                    StartDate = string.Format("{0:dd-MMM-yyy}", x.StartDate),
                    EndDate = string.Format("{0:dd-MMM-yyy}", x.EndDate)
                    //Status = ((DMPStatus)x.Status).ToString()
                });
            });

            return View(dmpVM);
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
                        Version = string.Format("{0}.{1}", x.Version, x.TempVersion),
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

        public ActionResult CreateNewDMP()
        {
            //CreateDMPViewModel _newDmpVM = new CreateDMPViewModel
            //{
            //    organizations = OrgsRepo()
            //};

            return View(OrgsRepo());
        }

        [HttpPost]
        public ActionResult DocumentWizardPage(DAL.Entities.DMP newDMP)
        {
            MyDMP = newDMP;
            MyDMP.DateCreated = DateTime.Now;
            MyDMP.CreatedBy = initiator;

            bool result = SaveOrUpdateDMP(true);
            if (result)
            {
                return Json(MyDMP.Id);
            }
            else
            {
                return new HttpStatusCodeResult(400, "DMP with this title already exists");
            }
        }

        public ActionResult DocumentWizardPage(int? dmpId)
        {
            if ((!dmpId.HasValue || dmpId.Value == 0))
            {
                return RedirectToAction("CreateNewDMP");
            }

            MyDMP = dmpDAO.Retrieve(dmpId.Value);

            CreateDocumentViewModel docVM = new CreateDocumentViewModel
            {
                Organization = MyDMP.Organization,
                Initiator = initiator,
            };
            return View(docVM);
        }

        [HttpPost]
        public ActionResult SaveNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF, Approval approval,
            VersionAuthor versionAuthor, VersionMetadata versionMetadata, Summary summary, RolesAndResponsiblities roleNresp,
            ReportData reportData, DataVerificaton dataVerification, DigitalData digital, NonDigitalData nonDigital,
            DataCollectionProcesses datacollectionProcesses, IntellectualPropertyCopyrightAndOwnership intelProp,
            DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt,
            DigitalDataRetention digitalDataRetention, NonDigitalDataRetention nonDigitalRetention)
        {


            ProjectDetails ProjDetails = projDTF;
            ProjDetails.Organization = MyDMP.Organization;
            ProjDetails.AbreviationOfImplementingPartner = MyDMP.Organization.ShortName;
            ProjDetails.NameOfImplementingPartner = MyDMP.Organization.Name;
            MyDMP.TheProject = ProjDetails;


            WizardPage page = new WizardPage
            {
                ProjectProfile = new ProjectProfile
                {
                    EthicalApproval = ethicsApproval,
                    ProjectDetails = ProjDetails,
                },
                DocumentRevisions = new List<DocumentRevisions>
                {
                    new DocumentRevisions{
                        Version = new DAL.Entities.Version
                        {
                            Approval = approval,
                            VersionAuthor = versionAuthor,
                            VersionMetadata = versionMetadata,
                        }
                    }
                },
                Planning = new Planning { Summary = summary },
                DataCollection = new DataCollection
                {
                    Report = new Report
                    {
                        ReportData = reportData,
                        RoleAndResponsibilities = roleNresp
                    },
                },
                QualityAssurance = new QualityAssurance { DataVerification = dataVerification },
                DataCollectionProcesses = datacollectionProcesses,
                DataStorage = new DataStorage
                {
                    Digital = digital,
                    NonDigital = nonDigital
                },
                IntellectualPropertyCopyrightAndOwnership = intelProp,
                DataAccessAndSharing = dataSharing,
                DataDocumentationManagementAndEntry = dataDocMgt,
                PostProjectDataRetentionSharingAndDestruction = new PostProjectDataRetentionSharingAndDestruction
                {
                    DataToRetain = doc.DataToRetain,
                    DigitalDataRetention = digitalDataRetention,
                    Licensing = doc.Licensing,
                    NonDigitalRentention = nonDigitalRetention,
                    Duration = doc.Duration,
                    PreExistingData = doc.PreExistingData
                }
            };


            SaveProject(ProjDetails);
            SaveOrUpdateDMP(false);
            SaveDMPDocument(doc.VersionNumber, page, MyDMP.Id);
            projDAO.CommitChanges();

            return Json("ok", JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditDocumentWizardPage(int? dmpId, string documnentId = null)
        {
            if ((documnentId == null))
            {
                return RedirectToAction("CreateNewDMP", "Home");
            }
            Guid dGuid = new Guid(documnentId);
            DMPDocument Doc = new DMPDocumentDAO().Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }
            MyDMP = Doc.TheDMP;

            var thepageDoc = Doc.Document;
            EditDocumentViewModel2 docVM = new EditDocumentViewModel2
            {
                versionAuthor = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionAuthor,
                datacollectionProcesses = thepageDoc.DataCollectionProcesses,
                dataDocMgt = thepageDoc.DataDocumentationManagementAndEntry,
                dataSharing = thepageDoc.DataAccessAndSharing,
                dataVerification = thepageDoc.QualityAssurance.DataVerification,
                digital = thepageDoc.DataStorage.Digital,
                nonDigital = thepageDoc.DataStorage.NonDigital,
                digitalDataRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.DigitalDataRetention,
                nonDigitalRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.NonDigitalRentention,
                ethicsApproval = thepageDoc.ProjectProfile.EthicalApproval,
                intelProp = thepageDoc.IntellectualPropertyCopyrightAndOwnership,
                ppData = thepageDoc.PostProjectDataRetentionSharingAndDestruction,
                projectDetails = thepageDoc.ProjectProfile.ProjectDetails,
                summary = thepageDoc.Planning.Summary,
                versionMetadata = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionMetadata,
                reportData = thepageDoc.DataCollection.Report.ReportData,
                roleNresp = thepageDoc.DataCollection.Report.RoleAndResponsibilities,
                documentID = Doc.Id.ToString(),
            };

            return View(docVM);
        }

        [HttpPost]
        public ActionResult EditDocumentNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF, Approval approval,
            VersionAuthor versionAuthor, VersionMetadata versionMetadata, Summary summary, RolesAndResponsiblities roleNresp,
            ReportData reportData, DataVerificaton dataVerification, DigitalData digital, NonDigitalData nonDigital,
            DataCollectionProcesses datacollectionProcesses, IntellectualPropertyCopyrightAndOwnership intelProp,
            DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt,
            DigitalDataRetention digitalDataRetention)
        {

            ProjectDetails ProjDetails = projDTF; ;
            Guid dGuid = new Guid(doc.documentID);
            var previousDoc = dmpDocDAO.Retrieve(dGuid);

            if (previousDoc.TheDMP.TheProject == null)
            {
                ProjDetails = projDTF;
                SaveProject(ProjDetails);
            } 
            else
            {
                ProjDetails.Id = previousDoc.TheDMP.TheProject.Id;
                projDAO.Update(ProjDetails);
            }




            WizardPage page = new WizardPage
            {
                ProjectProfile = new ProjectProfile
                {
                    EthicalApproval = ethicsApproval,
                    ProjectDetails = ProjDetails,
                },
                DocumentRevisions = new List<DocumentRevisions>
                {
                    new DocumentRevisions{
                        Version = new DAL.Entities.Version
                        {
                            Approval = approval,
                            VersionAuthor = versionAuthor,
                            VersionMetadata = versionMetadata,
                        }
                    }
                },
                Planning = new Planning { Summary = summary },
                DataCollection = new DataCollection
                {
                    Report = new Report
                    {
                        ReportData = reportData,
                        RoleAndResponsibilities = roleNresp
                    },
                },
                QualityAssurance = new QualityAssurance { DataVerification = dataVerification },
                DataCollectionProcesses = datacollectionProcesses,
                DataStorage = new DataStorage
                {
                    Digital = digital,
                    NonDigital = nonDigital
                },
                IntellectualPropertyCopyrightAndOwnership = intelProp,
                DataAccessAndSharing = dataSharing,
                DataDocumentationManagementAndEntry = dataDocMgt,
                PostProjectDataRetentionSharingAndDestruction = new PostProjectDataRetentionSharingAndDestruction
                {
                    DataToRetain = doc.DataToRetain,
                    DigitalDataRetention = digitalDataRetention,
                    Licensing = doc.Licensing,
                    NonDigitalRentention = new NonDigitalDataRetention
                    {
                        DataRention = doc.nonDigitalDataRetention
                    },
                    Duration = doc.Duration,
                    PreExistingData = doc.PreExistingData
                }
            };

            SaveDMPDocument(doc.VersionNumber, page, previousDoc.TheDMP.Id);
            projDAO.CommitChanges();

            return Json("ok", JsonRequestBehavior.AllowGet);
        }


        public void SaveDMPDocument(string VersionNumber, WizardPage documentPages, int dmpId)
        {
            DMPDocument Doc = dmpDocDAO.SearchMostRecentByDMP(dmpId);
            if (Doc == null)
            {
                Doc = new DMPDocument
                {
                    CreationDate = DateTime.Now,
                    Initiator = initiator,
                    InitiatorUsername = initiator.Username,
                    LastModifiedDate = DateTime.Now,
                    Status = DMPStatus.New,
                    Version = Convert.ToInt32(VersionNumber.Split('.')[0]),
                    TempVersion = Convert.ToInt32(VersionNumber.Split('.')[1]),
                    TheDMP = MyDMP,
                    ReferralCount = 0,
                    PageNumber = 0,
                    Document = documentPages,
                    ApprovedBy = null,
                };
                dmpDocDAO.Save(Doc);
            }
            else
            {
                Doc.LastModifiedDate = DateTime.Now;
                Doc.Document = documentPages;
                dmpDocDAO.Update(Doc);
            }
        }

        public void SaveProject(ProjectDetails theproject)
        {
            if (projDAO.SearchByName(theproject.ProjectTitle) == null)
            {
                projDAO.Save(theproject);
            }
        }

        public bool SaveOrUpdateDMP(bool save)
        {
            var tmpDMP = dmpDAO.SearchByName(MyDMP.DMPTitle);
            if (save && tmpDMP == null)
            {
                dmpDAO.Save(MyDMP);
                dmpDAO.CommitChanges();
                return true;
            }
            else if (!save)
            {
                try
                {
                    dmpDAO.Update(MyDMP);

                }
                catch { }
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}