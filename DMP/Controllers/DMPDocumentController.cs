using CommonUtil.DAO;
using CommonUtil.Utilities;
using DAL.DAO;
using DAL.Entities;
using ShieldPortal.Services;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class DMPDocumentController : Controller
    {
        DMPDAO dmpDAO = null;
        //static DAL.Entities.DMP MyDMP = null;
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
                return RedirectToAction("CreateDMP", "ShieldPortal");
            }
            DAL.Entities.DMP MyDMP = dmpDAO.Retrieve(dmpId.Value);
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
            var states = ExcelHelper.RetrieveStatesName();
            if (dmpDoc != null)
            {                
                var thepageDoc = dmpDoc.Document;
               
                EditDocumentViewModel2 docVM = new EditDocumentViewModel2
                {
                    reportingLevel = thepageDoc.MonitoringAndEvaluationSystems.Process !=null ? thepageDoc.MonitoringAndEvaluationSystems.Process.ReportLevel : new List<string>(),
                    states = states,
                    People = thepageDoc.MonitoringAndEvaluationSystems.People,
                    Equipment = thepageDoc.MonitoringAndEvaluationSystems.Equipment,
                    Environment = thepageDoc.MonitoringAndEvaluationSystems.Environment, 
                    processes = thepageDoc.MonitoringAndEvaluationSystems.Process,
                    dataCollation = thepageDoc.MonitoringAndEvaluationSystems.Process !=null ? thepageDoc.MonitoringAndEvaluationSystems.Process.DataCollation : new List<DataCollation>(),
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
                    projectDetails =  thepageDoc.ProjectProfile.ProjectDetails,
                    summary = thepageDoc.Planning.Summary, 
                    reportDataList = thepageDoc.DataProcesses.Reports.ReportData,
                    Trainings = thepageDoc.MonitoringAndEvaluationSystems.People!=null ? thepageDoc.MonitoringAndEvaluationSystems.People.Trainings : new List<Trainings>(),
                    roles = thepageDoc.MonitoringAndEvaluationSystems.People !=null ? thepageDoc.MonitoringAndEvaluationSystems.People.Roles : new List<StaffGrouping>(),
                    responsibilities = thepageDoc.MonitoringAndEvaluationSystems.People !=null ? thepageDoc.MonitoringAndEvaluationSystems.People.Responsibilities : new List<StaffGrouping>(),
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
                    states = states,
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
        public ActionResult SaveFileUpload(string documentId = "")
        {
            try
            {
                var files = Request.Files;
                if (files == null || files.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "no files uploaded");
                }
                string filepath = System.Web.Hosting.HostingEnvironment.MapPath("~/DMPFileUploads/_" + files[0].FileName + "_" + documentId);
                files[0].SaveAs(filepath);

                List<StaffGrouping> roles = null, responsibility = null;
                List<Trainings> training = null;
                new DAL.Services.DMPExcelFile().ExtractRoles(files[0].InputStream, out roles, out responsibility, out training);

                if(roles == null || roles.Count ==0 || responsibility ==null || responsibility.Count == 0 || training==null || training.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file uploaded");
                }
                else
                {
                    return Json(new { filelocation = filepath, roles = roles, responsibility = responsibility, trainings = training }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }


        [HttpPost]
        public ActionResult SaveNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF,
            Summary summary, Equipment equipment, List<DataCollection> DataCollection, List<StaffGrouping> roles, List<StaffGrouping> responsibilities,
            ReportData reportData, List<DataVerificaton> dataVerification, DigitalData digital, NonDigitalData nonDigital, List<DataCollation> dataCollation,
            Processes processes, IntellectualPropertyCopyrightAndOwnership intelProp, AreaCoveredByIP siteCount, List<string> reportingLevel,
            DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, List<Trainings> Trainings,
            DigitalDataRetention digitalDataRetention, NonDigitalDataRetention nonDigitalRetention, List<ReportData> reportDataList)
        {

            var dmpid = Convert.ToInt32(Request.UrlReferrer.Query.Split(new string[] { "?dmpId=" }, StringSplitOptions.RemoveEmptyEntries)[0]);
            DMP MyDMP = dmpDAO.Retrieve(dmpid);
            if (MyDMP == null)
            {
                return RedirectToAction("CreateNewDMP");
            }

            ProjectDetails ProjDetails = projDTF;
            ProjDetails.Organization = MyDMP.Organization;
            ProjDetails.AbreviationOfImplementingPartner = MyDMP.Organization.ShortName;
            ProjDetails.NameOfImplementingPartner = MyDMP.Organization.Name;
            ProjDetails.AddressOfOrganization = MyDMP.Organization.Address;
            ProjDetails.PhoneNumber = MyDMP.Organization.PhoneNumber;
            ProjDetails.LeadActivityManager = new ProfileDAO().Retrieve(doc.leadactivitymanagerId);

            processes.DataCollation = dataCollation;
            processes.ReportLevel = reportingLevel;
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
                        StaffingInformation = doc.StaffingInformation,
                        Roles = roles,
                        Responsibilities = responsibilities 
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
                MyDMP.TheProject = ProjDetails;
                projDAO.Save(MyDMP.TheProject);                
                               
                dmpDAO.Update(MyDMP); 

                var theDoc = SaveDMPDocument(page, MyDMP, Guid.Empty); // MyDMP.Id);
                dmpDocDAO.CommitChanges();

                var data = new { documentId = theDoc.Id, projectId = MyDMP.TheProject.Id };

                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                projDAO.RollbackChanges();
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult EditDocumentNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF,
           AreaCoveredByIP siteCount, Equipment equipment, Summary summary, List<StaffGrouping> roles, List<StaffGrouping> responsibilities,
           ReportData reportData, List<DataVerificaton> dataVerification, DigitalData digital, NonDigitalData nonDigital,
           Processes processes, IntellectualPropertyCopyrightAndOwnership intelProp, List<DataCollation> dataCollation, List<string> reportingLevel,
           DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, List<DataCollection> DataCollection,
           DigitalDataRetention digitalDataRetention, List<Trainings> Trainings, List<ReportData> reportDataList)
        {
            processes.DataCollation = dataCollation;
            processes.ReportLevel = reportingLevel;

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
                ProjDetails.DocumentTitle = projDTF.DocumentTitle;
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
                previousDoc.TheDMP.TheProject.DocumentTitle = projDTF.DocumentTitle;

                projDAO.ExplicitUpdate(previousDoc.TheDMP.TheProject);
                 
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
                        StaffingInformation = doc.StaffingInformation,
                        Roles = roles,
                        Responsibilities = responsibilities, 
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

            var theDoc = SaveDMPDocument(page, previousDoc.TheDMP, currentDocumentID, currentMetadata); // previousDoc.TheDMP.Id);

            try
            {
                projDAO.CommitChanges();

                var data = new { documentId = theDoc.Id, projectId = ProjDetails.Id };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                projDAO.RollbackChanges();
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }            
        }

        public DMPDocument SaveDMPDocument(WizardPage documentPages, DMP MyDMP, Guid documentId, VersionMetadata metadata =null) // int dmpId)
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
