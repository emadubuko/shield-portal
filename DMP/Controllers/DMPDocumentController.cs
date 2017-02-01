using CommonUtil.DAO;
using CommonUtil.Utilities;
using DAL.DAO;
using DAL.Entities;
using DMP.Services;
using DMP.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
                var states = ExcelHelper.RetrieveStatesName();
                EditDocumentViewModel2 docVM = new EditDocumentViewModel2
                {
                    states = states,
                    People = thepageDoc.MonitoringAndEvaluationSystems.People,
                    Equipment = thepageDoc.MonitoringAndEvaluationSystems.Equipment,
                    Environment = thepageDoc.MonitoringAndEvaluationSystems.Environment,
                    //versionAuthor = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionAuthor,
                    processes = thepageDoc.MonitoringAndEvaluationSystems.Process,
                    dataDocMgt = thepageDoc.DataStorageAccessAndSharing.DataDocumentationManagementAndEntry,
                    dataSharing = thepageDoc.DataStorageAccessAndSharing.DataAccessAndSharing,
                    dataVerification = thepageDoc.QualityAssurance.DataVerification,
                    digital = thepageDoc.DataStorageAccessAndSharing.Digital,
                    nonDigital = thepageDoc.DataStorageAccessAndSharing.NonDigital,
                    digitalDataRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.DigitalDataRetention,
                    nonDigitalRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.NonDigitalRentention,
                    ethicsApproval = thepageDoc.ProjectProfile.EthicalApproval,
                    intelProp = thepageDoc.IntellectualPropertyCopyrightAndOwnership,
                    ppData = thepageDoc.PostProjectDataRetentionSharingAndDestruction,
                    projectDetails = thepageDoc.ProjectProfile.ProjectDetails,
                    summary = thepageDoc.Planning.Summary, 
                    reportDataList = thepageDoc.DataProcesses.Reports.ReportData,
                    Trainings = thepageDoc.MonitoringAndEvaluationSystems.People.Trainings,
                    documentID = dmpDoc.Id.ToString(),
                    EditMode = true,
                    Profiles = profileDAO.RetrieveAll().ToDictionary(x => x.Id),
                    Organization = MyDMP.Organization,
                    dataCollection = thepageDoc.DataProcesses.DataCollection,
                    DataFlowChart = thepageDoc.MonitoringAndEvaluationSystems.People.DataFlowChart,
                };
                return View(docVM);
            }
            else
            { 
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
            Summary summary, Equipment equipment, DataCollection DataCollection,
            ReportData reportData, List<DataVerificaton> dataVerification, DigitalData digital, NonDigitalData nonDigital,
            Processes processes, IntellectualPropertyCopyrightAndOwnership intelProp, AreaCoveredByIP siteCount,
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
                DocumentRevisions = new List<DocumentRevisions>
                {
                    new DocumentRevisions{
                        Version = new DAL.Entities.Version
                        {
                            Approval = new Approval(),
                            VersionAuthor = GenerateVersionAuthor(),
                            VersionMetadata = GenerateMetaData(),
                        }
                    }
                },
                Planning = new Planning { Summary = summary },
                MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
                {
                    People = new People
                    {
                        Trainings = Trainings,
                        DataFlowChart = doc.DataFlowChart,
                        DataHandlingAndEntry = doc.DataHandlingAndEntry,
                        RoleAndResponsibilities = doc.RoleAndResponsibilities,
                        Staffing = doc.Staffing
                    },
                    Equipment = equipment,
                    Environment = new DAL.Entities.Environment
                    {
                        StatesCoveredByImplementingPartners = doc.StatesCoveredByImplementingPartners,
                     NumberOfSitesCoveredByImplementingPartners = siteCount
                    },
                    Process = processes
                },
                DataProcesses = new DataProcesses
                {
                    DataCollection = DataCollection,
                    Reports = new Report
                    {
                        ReportData = reportDataList
                    },
                },
                QualityAssurance = new QualityAssurance { DataVerification = dataVerification },
                DataStorageAccessAndSharing = new DataStorage
                {
                    Digital = digital,
                    NonDigital = nonDigital,
                    DataAccessAndSharing = dataSharing,
                    DataDocumentationManagementAndEntry = dataDocMgt,
                },
                IntellectualPropertyCopyrightAndOwnership = intelProp,
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
                projDAO.Save(ProjDetails);
                projDAO.CommitChanges();

                MyDMP.TheProject = ProjDetails;
                dmpDAO.Update(MyDMP);
                var theDoc = SaveDMPDocument(page, Guid.Empty); // MyDMP.Id);
                dmpDocDAO.CommitChanges();

                var data = new { documentId = theDoc.Id, projectId = ProjDetails.Id };

                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                projDAO.RollbackChanges();
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult EditDocumentNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF, Approval approval,
           AreaCoveredByIP siteCount, Equipment equipment, Summary summary,
           ReportData reportData, List<DataVerificaton> dataVerification, DigitalData digital, NonDigitalData nonDigital,
           Processes processes, IntellectualPropertyCopyrightAndOwnership intelProp,
           DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, DataCollection DataCollection,
           DigitalDataRetention digitalDataRetention, List<Trainings> Trainings, List<ReportData> reportDataList)
        {

            Guid dGuid = new Guid(doc.documentID);
            var previousDoc = dmpDocDAO.Retrieve(dGuid);
            ProjectDetails ProjDetails = projDTF;

            if (previousDoc.TheDMP.TheProject == null)
            {
                
                ProjDetails.Organization = previousDoc.TheDMP.Organization;
                ProjDetails.AbreviationOfImplementingPartner = previousDoc.TheDMP.Organization.ShortName;
                ProjDetails.NameOfImplementingPartner = previousDoc.TheDMP.Organization.Name;
                ProjDetails.AddressOfOrganization = previousDoc.TheDMP.Organization.Address;
                ProjDetails.PhoneNumber = previousDoc.TheDMP.Organization.PhoneNumber;
                ProjDetails.LeadActivityManager = new ProfileDAO().Retrieve(doc.leadactivitymanagerId);
                projDAO.Save(ProjDetails); 
            }
            else
            {                               
                previousDoc.TheDMP.TheProject.LeadActivityManager = new ProfileDAO().Retrieve(doc.leadactivitymanagerId);
                previousDoc.TheDMP.TheProject.GrantReferenceNumber = projDTF.GrantReferenceNumber; 
                previousDoc.TheDMP.TheProject.ProjectTitle = projDTF.ProjectTitle;
                previousDoc.TheDMP.TheProject.ProjectEndDate = projDTF.ProjectEndDate;
                previousDoc.TheDMP.TheProject.ProjectStartDate = projDTF.ProjectStartDate; 

                projDAO.Update(previousDoc.TheDMP.TheProject);

                ProjDetails = previousDoc.TheDMP.TheProject;
            }

            var revisions = GenerateDocumentRevision(previousDoc);
            WizardPage page = new WizardPage
            {
                ProjectProfile = new ProjectProfile
                {
                    EthicalApproval = ethicsApproval,
                    ProjectDetails = ProjDetails,
                },
                DocumentRevisions = revisions,
                Planning = new Planning { Summary = summary },
                MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
                {
                    People = new People
                    {
                        Trainings = Trainings,
                        DataFlowChart = doc.DataFlowChart,
                        DataHandlingAndEntry = doc.DataHandlingAndEntry,
                        RoleAndResponsibilities = doc.RoleAndResponsibilities,
                        Staffing = doc.Staffing
                    },
                    Equipment = equipment,
                    Environment = new DAL.Entities.Environment
                    {
                        StatesCoveredByImplementingPartners = doc.StatesCoveredByImplementingPartners,
                        NumberOfSitesCoveredByImplementingPartners = siteCount
                    },
                    Process = processes
                },
                DataProcesses = new DataProcesses
                {
                    DataCollection = DataCollection,
                    Reports = new Report
                    {
                        ReportData = reportDataList
                    },
                },
                QualityAssurance = new QualityAssurance { DataVerification = dataVerification },
                DataStorageAccessAndSharing = new DataStorage
                {
                    Digital = digital,
                    NonDigital = nonDigital,
                    DataAccessAndSharing = dataSharing,
                    DataDocumentationManagementAndEntry = dataDocMgt,
                },
                IntellectualPropertyCopyrightAndOwnership = intelProp,
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

            Guid currentDocumentID = previousDoc.Status == DMPStatus.Approved ? Guid.Empty : previousDoc.Id;

            VersionMetadata currentMetadata = null;
            if(revisions.LastOrDefault() !=null && revisions.LastOrDefault().Version != null)
            {
                currentMetadata = revisions.LastOrDefault().Version.VersionMetadata;
            }

            var theDoc = SaveDMPDocument(page, currentDocumentID, currentMetadata); // previousDoc.TheDMP.Id);

            try
            {
                projDAO.CommitChanges();

                var data = new { documentId = theDoc.Id, projectId = ProjDetails.Id };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                projDAO.RollbackChanges();
                return new HttpStatusCodeResult(400, ex.Message);
            }            
        }

        public DMPDocument SaveDMPDocument(WizardPage documentPages, Guid documentId, VersionMetadata metadata =null) // int dmpId)
        {
            DMPDocument Doc = dmpDocDAO.Retrieve(documentId); // dmpDocDAO.SearchMostRecentByDMP(dmpId);
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
                    Version = "0.1",
                    TheDMP = MyDMP,
                    ReferralCount = 0,
                    PageNumber = 0,
                    Document = documentPages,
                    ApprovedBy = null,
                    ApprovedDate = (DateTime)SqlDateTime.Null
                };
                dmpDocDAO.Save(Doc);
            }
            else
            {
                if(DateTime.MinValue >= Doc.CreationDate)
                {
                    Doc.CreationDate = DateTime.Now;
                }

                Doc.ApprovedDate = (DateTime)SqlDateTime.Null;
                Doc.LastModifiedDate = DateTime.Now;
                Doc.Document = documentPages;
                Doc.Version = metadata != null ? metadata.VersionNumber : "0.1";
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


        public List<DocumentRevisions> GenerateDocumentRevision(DMPDocument previousDoc)
        {
            var previousVersion = previousDoc.Document.DocumentRevisions.LastOrDefault();
            List<DocumentRevisions> revs = new List<DocumentRevisions>();
            int noOfPreviousRevision = previousDoc.Document.DocumentRevisions.Count - 1;
            revs.AddRange(previousDoc.Document.DocumentRevisions.Take(noOfPreviousRevision));

            DocumentRevisions currentRevision = new DocumentRevisions
            {
                Version = new DAL.Entities.Version
                {
                    Approval = new Approval(),
                    VersionAuthor = GenerateVersionAuthor(),
                    VersionMetadata = previousVersion != null && previousVersion.Version != null ? GenerateMetaData(previousVersion.Version.VersionMetadata) : GenerateMetaData(),
                }
            };
            revs.Add(currentRevision);
            return revs;
        }
        public VersionAuthor GenerateVersionAuthor()
        {
            var profile = new Utils().GetloggedInProfile();
            VersionAuthor author = new VersionAuthor
            {
                EmailAddressOfAuthor = profile.ContactEmailAddress,
                FirstNameOfAuthor = profile.FirstName,
                JobDesignation = profile.JobDesignation,
                OtherNamesOfAuthor = profile.OtherNames,
                PhoneNumberOfAuthor = profile.ContactPhoneNumber,
                SurnameAuthor = profile.Surname,
                TitleOfAuthor = profile.Title
            };
            return author;
        }

        public VersionMetadata GenerateMetaData(VersionMetadata previous = null)
        {
            string mainVersion = "0";
            int subVersion = 1;
            if (previous != null && !string.IsNullOrEmpty(previous.VersionNumber))
            {
                var t = previous.VersionNumber.Split('.');
                mainVersion = t[0];
                if (t.Count() > 1)
                {
                    subVersion = Convert.ToInt16(t[1]) + 1;
                }
            }
            VersionMetadata metaData = new VersionMetadata
            {
                VersionDate = DateTime.Now.ToShortDateString(),
                VersionNumber = mainVersion + "." + subVersion
            };
            return metaData;
        }
    }
}
