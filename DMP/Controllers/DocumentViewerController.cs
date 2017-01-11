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
    public class DocumentViewerController : Controller
    {
        static DMPDocument Doc = null;
        CommentDAO commentDAO = null;
        DMPDocumentDAO dmpDocumentDAO = null;

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

            VersionAuthor versionAuthor = null;
            VersionMetadata versionMetadata = null;
            Approval approval = new Approval();

            if (Doc.Status != DMPStatus.Approved)
            {
                versionAuthor = GenerateVersionAuthor(Doc.Initiator);
                versionMetadata = new VersionMetadata
                {
                    VersionDate = string.Format("{0:dd-MMM-yyyy}", Doc.CreationDate),
                    VersionNumber = "0.1"
                };
                
            }
            else
            {
                versionAuthor = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionAuthor;
                versionMetadata = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionMetadata;
                approval = thepageDoc.DocumentRevisions.LastOrDefault().Version.Approval;
                if (versionMetadata == null || string.IsNullOrEmpty(versionMetadata.VersionNumber))
                {
                    versionAuthor = GenerateVersionAuthor(Doc.Initiator);
                    versionMetadata = new VersionMetadata
                    {
                        VersionDate = string.Format("{0:dd-MMM-yyyy}", Doc.CreationDate),
                        VersionNumber = "0.1"
                    };
                }
            }


            EditDocumentViewModel2 docVM = new EditDocumentViewModel2
            {
                versionAuthor = versionAuthor,
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
                versionMetadata = versionMetadata,
                reportData = thepageDoc.DataCollection.Report.ReportData,
                roleNresp = thepageDoc.DataCollection.Report.RoleAndResponsibilities,
                documentID = Doc.Id.ToString(),
                Comments = comments,
                status = Doc.Status,
                approval = approval
            };

            return View(docVM);
        }

        [HttpPost]
        public ActionResult AddComment(Comment comment)
        {
            if(comment != null && !string.IsNullOrEmpty(comment.Message))
            {
                comment.DateAdded = string.Format("{0:dd-MMM-yyyy hh:mm:ss}", DateTime.Now);
                comment.Commenter = "guest";
                comment.DMPDocument = Doc;

                try
                {
                    commentDAO.Save(comment);
                    commentDAO.CommitChanges();
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
        public ActionResult DecineDocument(string documnentId = null)
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
                Doc.ApprovedBy = GetloggedInProfile();
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

            var previousRevision = Doc.Document.DocumentRevisions.LastOrDefault();
            Profile currentUser = GetloggedInProfile();

            VersionMetadata versionMetaData = new VersionMetadata
            {
                VersionDate = string.Format("{0:dd-MMM-yyyy}", DateTime.Now),
                VersionNumber = (Doc.Version + 1).ToString()
            };
            DocumentRevisions revision = new DocumentRevisions
            {
                Version = new DAL.Entities.Version
                {
                    Approval = GenerateApproval(currentUser),
                    VersionAuthor = GenerateVersionAuthor(Doc.Initiator),
                    VersionMetadata = versionMetaData
                }
            };
            try
            {
                Doc.Document.DocumentRevisions.Add(revision);
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

        VersionAuthor GenerateVersionAuthor(Profile Initiator)
        {
            VersionAuthor author = new VersionAuthor
            {
                EmailAddressOfAuthor = Initiator.ContactEmailAddress,
                FirstNameOfAuthor = Initiator.FirstName,
                JobDesignation = Initiator.JobDesignation,
                OtherNamesOfAuthor = Initiator.OtherNames,
                PhoneNumberOfAuthor = Initiator.ContactPhoneNumber,
                SurnameAuthor = Initiator.Surname,
                TitleOfAuthor = Initiator.Title
            };
            return author;
        }

        Approval GenerateApproval(Profile approver)
        {
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

        //this should get logged in User
        Profile GetloggedInProfile()
        {
            //return dummy for now 
            Guid pGuid = new Guid("D2ED8EA3-A335-4718-914D-A6F301671679");
            var result = new ProfileDAO().Retrieve(pGuid);
            return result;
        }

        [HttpPost]
        public ActionResult DownloadDocument()
        {


            return Json("ok");
        }


    }
}