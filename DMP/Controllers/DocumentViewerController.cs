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

        public DocumentViewerController()
        {
            commentDAO = new CommentDAO();
        }

        // GET: DocumentViewer
        public ActionResult DocumentPreview(string documnentId = null)
        {
            if ((documnentId == null))
            {
                return RedirectToAction("CreateNewDMP", "Home");
            }
            Guid dGuid = new Guid(documnentId);
            Doc = new DMPDocumentDAO().Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }

            var thepageDoc = Doc.Document;
            var comments = commentDAO.SearchByDocumentId(Doc.Id);
           
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
                Comments = comments
            };

            return View(docVM);
        }

        [HttpPost]
        public ActionResult AddComment(Comment comment)
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

    }
}