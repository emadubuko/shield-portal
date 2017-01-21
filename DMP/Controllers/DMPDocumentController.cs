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
    public class DMPDocumentController : Controller
    {
        DMPDAO dmpDAO = null;
        static DAL.Entities.DMP MyDMP = null;
        DMPDocumentDAO dmpDocDAO = null;
        OrganizationDAO orgDAO = null;
        ProjectDetailsDAO projDAO = null;
        ProfileDAO profileDAO = null;
 
        public DMPDocumentController()
        {
            dmpDAO = new DMPDAO();
            projDAO = new ProjectDetailsDAO();
            orgDAO = new OrganizationDAO();
            dmpDocDAO = new DMPDocumentDAO();
            profileDAO = new ProfileDAO();
        }
        // GET: DMPDocument
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WizardPages(int? dmpId, string documnentId = null)
        {
            if ((!dmpId.HasValue || dmpId.Value == 0))
            {
                return RedirectToAction("CreateDMP", "DMP");
            }
            MyDMP = dmpDAO.Retrieve(dmpId.Value);
            DMPDocument dmpDoc = null;
            if (!string.IsNullOrEmpty(documnentId))
            {
                Guid dGuid = new Guid(documnentId);
                dmpDoc = new DMPDocumentDAO().Retrieve(dGuid);
            }
            if (dmpDoc == null && MyDMP.DMPDocuments !=null && MyDMP.DMPDocuments.Count() > 0)
            {
                dmpDoc = MyDMP.DMPDocuments.LastOrDefault();
            }
                      
            if (dmpDoc != null)
            {                
                var thepageDoc = dmpDoc.Document;
                if(dmpDoc.Status == DMPStatus.Approved)
                {

                }

                EditDocumentViewModel2 docVM = new EditDocumentViewModel2
                {
                    //versionAuthor = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionAuthor,
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
                    //versionMetadata = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionMetadata,
                    reportDataList = thepageDoc.Reports != null ? thepageDoc.Reports.ReportData : new List<ReportData>(),
                    roleNresp = thepageDoc.MonitoringAndEvaluationSystems != null ? thepageDoc.MonitoringAndEvaluationSystems.RoleAndResponsibilities : new RolesAndResponsiblities(),
                    Trainings = thepageDoc.MonitoringAndEvaluationSystems != null ? thepageDoc.MonitoringAndEvaluationSystems.Trainings : new List<Trainings>(),
                    //reportData = thepageDoc.Reports.ReportData,
                    //roleNresp = thepageDoc.MonitoringAndEvaluationSystems.RoleAndResponsibilities,
                    //Trainings = thepageDoc.MonitoringAndEvaluationSystems.Trainings,
                    documentID = dmpDoc.Id.ToString(),
                    EditMode = true,
                    Profiles = profileDAO.RetrieveAll().ToDictionary(x => x.Id),
                    Organization = MyDMP.Organization,
                    dataCollection = thepageDoc.DataCollection,
                    DataFlowChart = thepageDoc.MonitoringAndEvaluationSystems != null ? thepageDoc.MonitoringAndEvaluationSystems.DataFlowChart : null,
                };
                return View(docVM);
            }
            else
            {
                CreateDocumentViewModel vm = new CreateDocumentViewModel();

                EditDocumentViewModel2 docVM = new EditDocumentViewModel2
                {
                    Organization = MyDMP.Organization,
                    Initiator = new Utils().GetloggedInProfile(),  
                    EditMode = false,
                    projectDetails = new ProjectDetails
                    {
                        Organization = MyDMP.Organization,
                        DocumentTitle = MyDMP.DMPTitle, 
                    },
                     Profiles = profileDAO.RetrieveAll().ToDictionary(x=>x.Id), 
                };
                return View(docVM);
            }
        }
        

        [HttpPost]
        public ActionResult SaveNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF,
            Summary summary, RolesAndResponsiblities roleNresp, List<DataCollection> DataCollection,
            ReportData reportData, List<DataVerificaton> dataVerification, DigitalData digital, NonDigitalData nonDigital,
            DataCollectionProcesses datacollectionProcesses, IntellectualPropertyCopyrightAndOwnership intelProp,
            DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, List<Trainings> Trainings,
            DigitalDataRetention digitalDataRetention, NonDigitalDataRetention nonDigitalRetention, List<ReportData> reportDataList)
        {
            //incase session has timed out
            if (MyDMP == null)
            {
                var dmpid = Convert.ToInt32(Request.UrlReferrer.Query.Split(new string[] { "?dmpId=" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                MyDMP = dmpDAO.Retrieve(dmpid);
                if (MyDMP == null)
                {
                    return RedirectToAction("CreateNewDMP");
                }
            }

            ProjectDetails ProjDetails = projDTF;
            ProjDetails.Organization = MyDMP.Organization;
            ProjDetails.AbreviationOfImplementingPartner = MyDMP.Organization.ShortName;
            ProjDetails.NameOfImplementingPartner = MyDMP.Organization.Name;
            ProjDetails.AddressOfOrganization = MyDMP.Organization.Address;
            ProjDetails.PhoneNumber = MyDMP.Organization.PhoneNumber;
            ProjDetails.LeadActivityManager = new ProfileDAO().Retrieve(doc.leadactivitymanagerId);

            WizardPage page = new WizardPage
            {
                ProjectProfile = new ProjectProfile
                {
                    EthicalApproval = ethicsApproval,
                    ProjectDetails = ProjDetails,
                },
                Planning = new Planning { Summary = summary },
                DataCollection = DataCollection,
                MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
                {
                    DataFlowChart = doc.DataFlowChart,
                    RoleAndResponsibilities = roleNresp,
                    Trainings = Trainings,
                },
                Reports = new Report
                {
                    ReportData = reportDataList
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


            try
            {
                bool saved = SaveProject(ProjDetails);
                if (saved || MyDMP.TheProject != null)
                {
                    projDAO.CommitChanges();

                    MyDMP.TheProject = ProjDetails;
                    dmpDAO.Update(MyDMP);
                    //SaveOrUpdateDMP(false);
                    var theDoc = SaveDMPDocument(page, MyDMP.Id);
                    dmpDocDAO.CommitChanges();

                    var data = new { documentId = theDoc.Id, projectId = ProjDetails.Id };

                    return Json(data, JsonRequestBehavior.AllowGet);
                    //return Json(theDoc, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("project with the same name already exist", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                projDAO.RollbackChanges();
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult EditDocumentNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF, Approval approval,
           VersionAuthor versionAuthor, VersionMetadata versionMetadata, Summary summary, RolesAndResponsiblities roleNresp,
           ReportData reportData, List<DataVerificaton> dataVerification, DigitalData digital, NonDigitalData nonDigital,
           DataCollectionProcesses datacollectionProcesses, IntellectualPropertyCopyrightAndOwnership intelProp,
           DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, List<DataCollection> DataCollection,
           DigitalDataRetention digitalDataRetention, List<Trainings> Trainings, List<ReportData> reportDataList)
        {

            Guid dGuid = new Guid(doc.documentID);
            var previousDoc = dmpDocDAO.Retrieve(dGuid);

            ProjectDetails ProjDetails = projDTF;
            ProjDetails.Organization = previousDoc.TheDMP.Organization;
            ProjDetails.AbreviationOfImplementingPartner = previousDoc.TheDMP.Organization.ShortName;
            ProjDetails.NameOfImplementingPartner = previousDoc.TheDMP.Organization.Name;
            ProjDetails.AddressOfOrganization = previousDoc.TheDMP.Organization.Address;
            ProjDetails.PhoneNumber = previousDoc.TheDMP.Organization.PhoneNumber;
            ProjDetails.LeadActivityManager = new ProfileDAO().Retrieve(doc.leadactivitymanagerId);

             
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
                DataCollection = DataCollection,
                MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
                {
                     DataFlowChart = doc.DataFlowChart,
                      RoleAndResponsibilities = roleNresp,
                    Trainings = Trainings,
                },
                 Reports = new Report
                 {
                      ReportData = reportDataList
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

            var theDoc = SaveDMPDocument(page, previousDoc.TheDMP.Id);
            projDAO.CommitChanges();

            var data = new { documentId = theDoc.Id, projectId = ProjDetails.Id };
            return Json(data, JsonRequestBehavior.AllowGet); 
        }

        public DMPDocument SaveDMPDocument(WizardPage documentPages, int dmpId)
        {
            DMPDocument Doc = dmpDocDAO.SearchMostRecentByDMP(dmpId);
            var currentUser = new Utils().GetloggedInProfile();
            if (Doc == null)
            {
                Doc = new DMPDocument
                {
                    CreationDate = DateTime.Now,
                    Initiator = currentUser,  
                    InitiatorUsername = currentUser.Username,
                    LastModifiedDate = DateTime.Now,
                    Status = DMPStatus.New,
                    Version = 0,
                    TempVersion = 1,
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
            return Doc;
        }

        public bool SaveProject(ProjectDetails theproject)
        {
            bool saved = false;
            if (projDAO.SearchByName(theproject.ProjectTitle) == null)
            {
                projDAO.Save(theproject);
                saved = true;
            }
            return saved;
        }


    }
}

//public ActionResult EditDocumentWizardPage(int? dmpId, string documnentId = null)
//{
//    if ((documnentId == null))
//    {
//        return RedirectToAction("CreateNewDMP", "Home");
//    }
//    Guid dGuid = new Guid(documnentId);
//    DMPDocument Doc = new DMPDocumentDAO().Retrieve(dGuid);

//    if (Doc == null)
//    {
//        return new HttpStatusCodeResult(400, "Bad request");
//    }
//    MyDMP = Doc.TheDMP;

//    var thepageDoc = Doc.Document;
//    EditDocumentViewModel2 docVM = new EditDocumentViewModel2
//    {
//        //versionAuthor = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionAuthor,
//        datacollectionProcesses = thepageDoc.DataCollectionProcesses,
//        dataDocMgt = thepageDoc.DataDocumentationManagementAndEntry,
//        dataSharing = thepageDoc.DataAccessAndSharing,
//        dataVerification = thepageDoc.QualityAssurance.DataVerification,
//        digital = thepageDoc.DataStorage.Digital,
//        nonDigital = thepageDoc.DataStorage.NonDigital,
//        digitalDataRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.DigitalDataRetention,
//        nonDigitalRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.NonDigitalRentention,
//        ethicsApproval = thepageDoc.ProjectProfile.EthicalApproval,
//        intelProp = thepageDoc.IntellectualPropertyCopyrightAndOwnership,
//        ppData = thepageDoc.PostProjectDataRetentionSharingAndDestruction,
//        projectDetails = thepageDoc.ProjectProfile.ProjectDetails,
//        summary = thepageDoc.Planning.Summary,
//        //versionMetadata = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionMetadata,
//        reportData = thepageDoc.DataCollection.Report.ReportData,
//        roleNresp = thepageDoc.DataCollection.Report.RoleAndResponsibilities,
//        documentID = Doc.Id.ToString(),
//    };

//    return View(docVM);
//}