using DAL.DAO;
using DAL.Entities;
using DMP.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using DMP.Services;
using System.Collections.Generic;

namespace DMP.Controllers
{
    [Authorize]
    public class DocumentViewerController : Controller
    {
        static DMPDocument Doc = null;
        CommentDAO commentDAO = null;
        DMPDocumentDAO dmpDocumentDAO = null;

        PDFUtilities pdfUtil = new PDFUtilities();


        public DocumentViewerController()
        {
            commentDAO = new CommentDAO();
            dmpDocumentDAO = new DMPDocumentDAO();
        }

        // GET: DocumentViewer
        public ActionResult DocumentPreview(string documnentId = null)
        {
            if ((documnentId == null))
            {
                return RedirectToAction("CreateNewDMP", "Home");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = dmpDocumentDAO.Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }

            var thepageDoc = Doc.Document;
            var comments = commentDAO.SearchByDocumentId(Doc.Id);
 

            VersionAuthor versionAuthor =  new VersionAuthor();
            VersionMetadata versionMetadata = new VersionMetadata();
            Approval approval = new Approval();
             

            EditDocumentViewModel2 docVM = new EditDocumentViewModel2
            {
                 documentRevisions = Doc.Document.DocumentRevisions,
                versionAuthor = versionAuthor,
                datacollectionProcesses = thepageDoc.DataCollectionProcesses,
                dataCollection = thepageDoc.DataCollection,
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
                versionMetadata = versionMetadata,
                reportDataList = thepageDoc.Reports !=null ? thepageDoc.Reports.ReportData : new  List<ReportData>(),
                roleNresp = thepageDoc.MonitoringAndEvaluationSystems !=null ? thepageDoc.MonitoringAndEvaluationSystems.RoleAndResponsibilities: new RolesAndResponsiblities(),
                Trainings = thepageDoc.MonitoringAndEvaluationSystems !=null ? thepageDoc.MonitoringAndEvaluationSystems.Trainings: new List<Trainings>(),
                documentID = Doc.Id.ToString(),
                dmpId = Doc.TheDMP.Id,
                Comments = comments,
                status = Doc.Status,
                approval = approval,
                ProjectSummary = thepageDoc.ProjectProfile.ProjectDetails.ProjectSummary,
                Organization = Doc.TheDMP.Organization,
                DataFlowChart = thepageDoc.MonitoringAndEvaluationSystems != null ? thepageDoc.MonitoringAndEvaluationSystems.DataFlowChart : null,
            };

            return View(docVM);
        }

        [HttpPost]
        public ActionResult AddComment(Comment comment)
        {
            if (comment != null && !string.IsNullOrEmpty(comment.Message))
            {
                comment.DateAdded = string.Format("{0:dd-MMM-yyyy hh:mm:ss}", DateTime.Now);
                comment.Commenter = "guest";
                comment.DMPDocument = Doc;

                try
                {
                    commentDAO.Save(comment);
                    commentDAO.CommitChanges();
                    comment.DMPDocument = null; //remove this to limit the data being tranmitted back
                    return Json(comment);
                }
                catch (Exception ex)
                {
                    commentDAO.RollbackChanges();
                    return new HttpStatusCodeResult(400, ex.Message);
                }
            }
            else
            {
                return Json("empty comment");
            }

        }

        [HttpPost]
        public ActionResult DeclineDocument(string documnentId = null)
        {
            if ((documnentId == null))
            {
                return RedirectToAction("CreateNewDMP", "Home");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = dmpDocumentDAO.Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }
            try
            {
                Doc.ApprovedBy = new Utils().GetloggedInProfile();
                Doc.Status = DMPStatus.Rejected;
                Doc.ApprovedDate = DateTime.Now;

                dmpDocumentDAO.Update(Doc);
                dmpDocumentDAO.CommitChanges();
                return Json(Doc.Status);
            }
            catch (Exception ex)
            {
                dmpDocumentDAO.RollbackChanges();
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult ApproveDocument(string documnentId = null)
        {
            if ((documnentId == null))
            {
                return RedirectToAction("CreateNewDMP", "Home");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = dmpDocumentDAO.Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }
             
            Profile currentUser = new Utils().GetloggedInProfile();
              
            try
            {
                Doc.Document.DocumentRevisions = GenerateApprovedDocumentRevision(Doc);
                Doc.ApprovedBy = currentUser;
                Doc.Status = DMPStatus.Approved;
                Doc.ApprovedDate = DateTime.Now;

                dmpDocumentDAO.Update(Doc);
                dmpDocumentDAO.CommitChanges();
                return Json(((DMPStatus)Doc.Status).ToString());
            }
            catch (Exception ex)
            {
                dmpDocumentDAO.RollbackChanges();
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }


        public List<DocumentRevisions> GenerateApprovedDocumentRevision(DMPDocument previousDoc)
        {
            var previousVersion = previousDoc.Document.DocumentRevisions.LastOrDefault();
            List<DocumentRevisions> revs = new List<DocumentRevisions>();
            int noOfPreviousRevision = previousDoc.Document.DocumentRevisions.Count - 1;
            revs.AddRange(previousDoc.Document.DocumentRevisions.Take(noOfPreviousRevision));

            DocumentRevisions currentRevision = new DocumentRevisions
            {
                Version = new DAL.Entities.Version
                {
                    Approval = GenerateApproval(),
                    VersionAuthor = previousVersion.Version.VersionAuthor,
                    VersionMetadata = previousVersion != null && previousVersion.Version != null ? GenerateApprovedVersionMetaData(previousVersion.Version.VersionMetadata) : GenerateApprovedVersionMetaData(),
                }
            };
            revs.Add(currentRevision);
            return revs;
        }

        public VersionMetadata GenerateApprovedVersionMetaData(VersionMetadata previous = null)
        {
            int mainVersion = 0;
            int subVersion = 0;
            if (previous != null)
            {
                var t = previous.VersionNumber.Split('.');
                mainVersion = Convert.ToInt16(t[0]) + 1;                 
            }
            VersionMetadata metaData = new VersionMetadata
            {
                VersionDate = DateTime.Now.ToShortDateString(),
                VersionNumber = mainVersion + "." + subVersion
            };
            return metaData;
        }
         

        Approval GenerateApproval()
        {
            Profile approver = new Utils().GetloggedInProfile();
            Approval _approval = new Approval
            {
                EmailaddressofApprover = approver.ContactEmailAddress,
                FirstnameofApprover = approver.FirstName,
                JobdesignationApprover = approver.JobDesignation,
                OthernamesofApprover = approver.OtherNames,
                PhonenumberofApprover = approver.ContactPhoneNumber,
                SurnameApprover = approver.Surname,
                TitleofApprover = approver.Title,
            };
            return _approval;
        }

        
        

        [HttpPost]
        public ActionResult DownloadDocument(string documnentId)
        {
            if ((documnentId == null))
            {
                return new HttpStatusCodeResult(400, "invalid documnent Id");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = dmpDocumentDAO.Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "invalid documnent Id");
            }

            string fileName = Doc.DocumentTitle+ ".pdf";
            string fullFilename = System.Web.Hosting.HostingEnvironment.MapPath("~/Downloads/" + fileName);

            using (FileStream fs = new FileStream(fullFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                Document doc = new Document(new Rectangle(PageSize.A4), 10f, 10f, 80f, 50f);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                writer.PageEvent = new ITextEvents();
                doc.Open();

                pdfUtil.GeneratePDFDocument(Doc, ref doc);

                doc.Close();
            }                
            return Json(fileName);
        }



        /// <summary>
        /// ///////////////this is dummmy data for testing
        /// </summary>

        WizardPage dummyData = new WizardPage
        {
            ProjectProfile = new ProjectProfile
            {
                EthicalApproval = new EthicsApproval
                {
                    EthicalApprovalForTheProject = @"Program must be of social or scientific value to either participants, the population they represent, the local community, the host country or the world.",
                    TypeOfEthicalApproval = "General",
                },
                ProjectDetails = new ProjectDetails
                {
                    ProjectSummary = @"Strengthening HIV Field Epidemiology Infectious Disease Surveillance and Lab Diagnostic Program [SHIELD] is a 5 years Health system strengthening project to be carried out by the University of Maryland Baltimore under the Division of Epidemiology and the Division of Clinical Care and Research",
                    AbreviationOfImplementingPartner = "CCCRN",
                    AddressOfOrganization = "Jahi district",
                    NameOfImplementingPartner = "Center for clinical research nigeria",
                    DocumentTitle = "SEED DMP for CCCRN",
                    ProjectTitle = "Strengthening HIV Field Epidemiology Infectious Disease Surveillance &Lab Diagnostic Program(SHIELD)",
                    ProjectEndDate = string.Format("{0:dd-MMM-yyyy}", DateTime.Now.AddMonths(7)),
                    ProjectStartDate = string.Format("{0:dd-MMM-yyyy}", DateTime.Now.AddMonths(-2)),
                    MissionPartner = "University of Maryland Baltimore"
                },
            },
            DocumentRevisions = new List<DocumentRevisions>
            {
                new DocumentRevisions{
                Version = new DAL.Entities.Version
                {
                    Approval = new Approval
                    {
                        SurnameApprover = "madubuko",
                        FirstnameofApprover = "Emeka"
                    },
                    VersionAuthor = new VersionAuthor
                    {
                        SurnameAuthor = "Madubuko",
                        FirstNameOfAuthor = "Christian"
                    },
                    VersionMetadata = new VersionMetadata
                    {
                        VersionDate = DateTime.Now.ToShortDateString(),
                        VersionNumber = "1.0"
                    },
                },
                },
                new DocumentRevisions{
                Version = new DAL.Entities.Version
                {
                    Approval = new Approval
                    {
                        SurnameApprover = "John",
                        FirstnameofApprover = "Doe"
                    },
                    VersionAuthor = new VersionAuthor
                    {
                        SurnameAuthor = "Madubuko",
                        FirstNameOfAuthor = "Christian"
                    },
                    VersionMetadata = new VersionMetadata
                    {
                        VersionDate = DateTime.Now.ToShortDateString(),
                        VersionNumber = "2.0"
                    },
                },
                }
            },
            Planning = new Planning
            {
                Summary = new Summary
                { ProjectObjectives = "TL;DR. Too long dont read" }
            },
            //DataCollection = new DataCollection
            //{
            //     DataSources = "Unknown"
            //},
             MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
             {
                  Trainings = new  List<Trainings>(),
                   DataFlowChart = "",
                 RoleAndResponsibilities = new RolesAndResponsiblities
                 {
                      AggregationLevel = "Determines the report",
                      CentralNationalLevel = "Archives",
                      HealthFacilityLevel = "Generates the report",                      
                 }
             },
            Reports = new Report
             {
                  ReportData = new List<ReportData>(),
                //ReportData
                //  { 
                //      NameOfReport = "Test report",
                //      DurationOfReporting = "steady",
                //  }
             },
            QualityAssurance = new QualityAssurance
            {
                //DataVerification = new DataVerificaton
                //{
                //    FormsOfDataVerification = "Manual",
                //    TypesOfDataVerification = "DQA"
                //},
            },
            DataCollectionProcesses = new DataCollectionProcesses
            {
                DataCollectionProcessess = "Collected by hand"
            },
            DataStorage = new DataStorage
            {
                Digital = new DigitalData
                {
                    Backup = "None",
                    Storagetype = "DBs"
                },
                NonDigital = new NonDigitalData
                {
                    NonDigitalDataTypes = "Registers",
                    StorageLocation = "File Cabinet"
                }
            },
            IntellectualPropertyCopyrightAndOwnership = new IntellectualPropertyCopyrightAndOwnership
            {
                ContractsAndAgreements = "None",
                Ownership = "Fully Us",
                UseOfThirdPartyDataSources = "None Needed"
            },
            DataAccessAndSharing = new DataAccessAndSharing
            {
                DataAccess = "Everyone with Login",
                DataSharingPolicies = "Only staff",
                DataTransmissionPolicies = "SSL Secured",
                SharingPlatForms = "Mobile"
            },
            DataDocumentationManagementAndEntry = new DataDocumentationManagementAndEntry
            {
                NamingStructureAndFilingStructures = "camel Case Name, arranged Alphabetical order",
                StoredDocumentationAndDataDescriptors = "Dont know"
            },
            PostProjectDataRetentionSharingAndDestruction = new PostProjectDataRetentionSharingAndDestruction
            {
                DataToRetain = "None",
                DigitalDataRetention = new DigitalDataRetention
                {
                    DataRetention = "Yes, anticipated"
                },
                Licensing = "MIT",
                NonDigitalRentention = new NonDigitalDataRetention
                {
                    DataRention = "In place"
                },
                Duration = "As long as relevant",
                PreExistingData = "Nope"
            }
        };

    }
}