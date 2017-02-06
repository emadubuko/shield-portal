using DAL.DAO;
using DAL.Entities;
using ShieldPortal.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using ShieldPortal.Services;
using System.Collections.Generic;
using System.Data.SqlTypes;
using CommonUtil.Entities;
using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Utilities;

namespace ShieldPortal.Controllers
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


            VersionAuthor versionAuthor = new VersionAuthor();
            VersionMetadata versionMetadata = new VersionMetadata();
            Approval approval = new Approval();


            EditDocumentViewModel2 docVM = new EditDocumentViewModel2
            {               
                Equipment = thepageDoc.MonitoringAndEvaluationSystems.Equipment,
                 Environment = thepageDoc.MonitoringAndEvaluationSystems.Environment,
                People = thepageDoc.MonitoringAndEvaluationSystems.People,
                documentRevisions = Doc.Document.DocumentRevisions,
                versionAuthor = versionAuthor,
                processes = thepageDoc.MonitoringAndEvaluationSystems.Process,
                 dataCollation = thepageDoc.MonitoringAndEvaluationSystems.Process !=null ? thepageDoc.MonitoringAndEvaluationSystems.Process.DataCollation : new List<DataCollation>(),
                dataCollection = thepageDoc.DataProcesses.DataCollection,
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
                versionMetadata = versionMetadata,
                reportDataList =  thepageDoc.DataProcesses.Reports.ReportData,
                Trainings = thepageDoc.MonitoringAndEvaluationSystems.People.Trainings,
                documentID = Doc.Id.ToString(),
                dmpId = Doc.TheDMP.Id,
                Comments = comments,
                status = Doc.Status,
                approval = approval,
                ProjectSummary = thepageDoc.ProjectProfile.ProjectDetails.ProjectSummary,
                Organization = Doc.TheDMP.Organization,
                DataFlowChart = thepageDoc.MonitoringAndEvaluationSystems.People.DataFlowChart,
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
                    Logger.LogError(ex);
                    return new HttpStatusCodeResult(400, ex.Message);
                }
            }
            else
            {
                return Json("empty comment");
            }

        }

        [HttpPost]
        public ActionResult ReferBack(string documnentId)
        {
            if ((documnentId == null))
            {
                return new HttpStatusCodeResult(400, "invalid document id");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = dmpDocumentDAO.Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }
            if (Doc.Status != DMPStatus.PendingApproval)
            {
                return new HttpStatusCodeResult(400, "Document is not submitted yet");
            }
            try
            {
                Doc.ApprovedBy = null;
                Doc.Status = DMPStatus.ReferredBack;
                Doc.ApprovedDate = (DateTime)SqlDateTime.Null;

                dmpDocumentDAO.Update(Doc);
                dmpDocumentDAO.CommitChanges();
                return Json("The document has been referred back");
            }
            catch (Exception ex)
            {
                dmpDocumentDAO.RollbackChanges();
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Submit(string documnentId)
        {
            if ((documnentId == null))
            {
                return new HttpStatusCodeResult(400, "invalid document id");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = dmpDocumentDAO.Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }
            try
            {
                Doc.ApprovedBy = null;
                Doc.Status = DMPStatus.PendingApproval;
                Doc.ApprovedDate = (DateTime)SqlDateTime.Null;

                dmpDocumentDAO.Update(Doc);
                dmpDocumentDAO.CommitChanges();
                return Json("The document has been submitted");
            }
            catch (Exception ex)
            {
                dmpDocumentDAO.RollbackChanges();
                Logger.LogError(ex);
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
            if (Doc.Status != DMPStatus.PendingApproval)
            {
                return new HttpStatusCodeResult(400, "Document is not submitted yet");
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
                Logger.LogError(ex);
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

            string fileName = Doc.DocumentTitle + ".pdf";
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

    }
}