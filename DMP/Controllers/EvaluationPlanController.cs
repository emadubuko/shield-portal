using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using EP.DAL.DAO;
using EP.DAL.Entities;
using Newtonsoft.Json;
using ShieldPortal.Services;
using ShieldPortal.ViewModel.EP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class EvaluationPlanController : Controller
    {
        // GET: EvaluationPlan
        public ActionResult Index()
        {
            var evaluations = new EvaluationDAO().RetrieveAll();
            return View(evaluations);
        }

        public ActionResult CreatePlan()
        {
            var profile = new Utils().GetloggedInProfile();
            ViewBag.Organization = profile.Organization.Name;
            return View();
        }

        public ActionResult EvaluationEdit(int Id)
        {
            Evaluation ep = new EvaluationDAO().Retrieve(Id);
            if (ep.SupplementaryInfo == null)
                ep.SupplementaryInfo = new SupplementaryInfo();
            if (ep.SupplementaryInfo.Info == null)
                ep.SupplementaryInfo.Info = new List<info>();
            var theActivity = (from x in ep.Activities
                               select
                                   new ActivityViewModel
                                   {
                                       id = x.Id,
                                       Name = x.Name,
                                       StartDate = string.Format("{0:dd-MMM-yyyy}", x.StartDate),
                                       EndDate = string.Format("{0:dd-MMM-yyyy}", x.EndDate),
                                       ExpectedOutcome = x.ExpectedOutcome,
                                       Status = Enum.GetName(typeof(EPStatus), x.Status).Replace("_", " "),
                                       Comments = (from y in x.Comments
                                                   select
                                                   new CommentViewModel
                                                   {
                                                       id = y.Id,
                                                       activityId = x.Id,
                                                       commenter = y.Commenter.FullName,
                                                       dateadded = y.DateAdded,
                                                       message = y.Message
                                                   })
                                   });
            return View(new EPDetailsViewModel { Activities = theActivity, Evaluation = ep });
        }

        public ActionResult EvaluationDetails(int Id)
        {
            Evaluation ep = new EvaluationDAO().Retrieve(Id);
            if (ep.SupplementaryInfo == null)
                ep.SupplementaryInfo = new SupplementaryInfo();
            if (ep.SupplementaryInfo.Info == null)
                ep.SupplementaryInfo.Info = new List<info>();

            var theActivity = (from x in ep.Activities
                               select
                                   new ActivityViewModel
                                   {
                                       id = x.Id,
                                       Name = x.Name,
                                       StartDate = string.Format("{0:dd-MMM-yyyy}", x.StartDate),
                                       EndDate = string.Format("{0:dd-MMM-yyyy}", x.EndDate),
                                       ExpectedOutcome = x.ExpectedOutcome,
                                       Status = Enum.GetName(typeof(EPStatus), x.Status).Replace("_", " "),
                                       Comments = (from y in x.Comments
                                                   select
                                                   new CommentViewModel
                                                   {
                                                       id = y.Id,
                                                       activityId = x.Id,
                                                       commenter = y.Commenter.FullName,
                                                       dateadded = y.DateAdded,
                                                       message = y.Message
                                                   })
                                   });
            return View(new EPDetailsViewModel { Activities = theActivity, Evaluation = ep });
        }

        [HttpPost]
        public ActionResult SaveEP(Evaluation EvaluationPlan)
        {
            if (EvaluationPlan == null || string.IsNullOrEmpty(EvaluationPlan.ProgramName))
            {
                return new HttpStatusCodeResult(400, "Please provide a valid program name");
            }
            var profile = new Utils().GetloggedInProfile();
            EvaluationPlan.DateCreated = DateTime.Now;
            EvaluationPlan.CreatedBy = profile;
            EvaluationPlan.ImplementingPartner = OrgsRepo().FirstOrDefault(x => x.Id == profile.Organization.Id);

            EvaluationDAO epDAO = new EvaluationDAO();
            var ep = epDAO.SearchByProgramName(EvaluationPlan.ProgramName, EvaluationPlan.ImplementingPartner.Id);
            if (ep == null || ep.Count() == 0)
            {
                EvaluationPlan.Activities.ToList().ForEach(f =>
                    {
                        f.TheEvaluation = EvaluationPlan;
                    });
                EvaluationPlan.Status = EPStatus.Yet_to_start;
                epDAO.Save(EvaluationPlan);
                epDAO.CommitChanges();
                return Json(EvaluationPlan.Id);
            }
            else
            {
                return new HttpStatusCodeResult(400, "This program Name already exist with this title already exists");
            }
        }


        [HttpPost]
        public ActionResult UpdateEP(Evaluation updatedPlan)
        {
            if (updatedPlan == null || string.IsNullOrEmpty(updatedPlan.ProgramName))
            {
                return new HttpStatusCodeResult(400, "Please provide a valid program name");
            }
            EvaluationDAO epDAO = new EvaluationDAO();

            Evaluation Ep = epDAO.Retrieve(updatedPlan.Id);
            Ep.LastUpdatedDate = DateTime.Now;
            Ep.ExpectedOutcome = updatedPlan.ExpectedOutcome;
            Ep.ProgramName = updatedPlan.ProgramName;
            Ep.StartDate = updatedPlan.StartDate;
            Ep.EndDate = updatedPlan.EndDate;

            foreach (var item in updatedPlan.Activities)
            {
                var previous_activity = Ep.Activities.FirstOrDefault(x => x.Id == item.Id);
                if (previous_activity != null)
                {
                    previous_activity.Name = item.Name;
                    previous_activity.StartDate = item.StartDate;
                    previous_activity.EndDate = item.EndDate;
                    previous_activity.ExpectedOutcome = item.ExpectedOutcome;
                }
                else
                {
                    Ep.Activities.Add(item);
                }
            }

            Ep.Activities.ToList().ForEach(f =>
                {
                    f.TheEvaluation = Ep;
                });

            epDAO.Save(Ep);
            epDAO.CommitChanges();
            return Json(Ep.Id);
        }

        [HttpPost]
        public ActionResult UpdateComment(string message, int commentId)
        {
            if (string.IsNullOrEmpty(message) || commentId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }
            EPCommentDAO dao = new EPCommentDAO();
            var comment = dao.Retrieve(commentId);
            if (comment == null)
            {
                return new HttpStatusCodeResult(400, "Invalid request");
            }
            comment.Message = message;
            try
            {
                dao.Update(comment);
                dao.CommitChanges();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                dao.RollbackChanges();
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult UpdateActivityStatus(string status, int activityId)
        {
            if (string.IsNullOrEmpty(status))
            {
                return new HttpStatusCodeResult(HttpStatusCode.PartialContent);
            }
            EvaluationActivityDAO dao = new EvaluationActivityDAO();
            EvaluationActivities _activity = dao.Retrieve(activityId);
            if (_activity == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.PartialContent);
            }
            _activity.Status = (EPStatus)Enum.Parse(typeof(EPStatus), status, true);
            try
            {
                dao.Update(_activity);
                dao.CommitChanges();

                return Json(Enum.GetName(typeof(EPStatus), _activity.Status).Replace("_", " "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public ActionResult AddComment(string message, int activityId)
        {
            if (string.IsNullOrEmpty(message) || activityId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.PartialContent);
            }
            var _activity = new EvaluationActivityDAO().Retrieve(activityId);
            if (_activity == null)
            {
                return new HttpStatusCodeResult(400, "Invalid request");
            }

            EPComment comment = new EPComment
            {
                DateAdded = string.Format("{0:dd-MMM-yyyy hh:mm:ss}", DateTime.Now),
                Commenter = new Utils().GetloggedInProfile(),
                TheActivity = _activity,
                Message = message
            };

            EPCommentDAO dao = new EPCommentDAO();
            try
            {
                dao.Save(comment);
                dao.CommitChanges();
                return Json(new
                {
                    id = comment.Id,
                    activityId = _activity.Id,
                    dateadded = comment.DateAdded,
                    commenter = comment.Commenter.FullName,
                    message = comment.Message,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                dao.RollbackChanges();
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult addsupplementaryinfo(info info) //(string title, string content, int evaluationId)
        {
            if (info == null && string.IsNullOrEmpty(info.Content) || string.IsNullOrEmpty(info.Title))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NoContent);
            }
            EvaluationDAO dao = new EvaluationDAO();
            var ep = new EvaluationDAO().Retrieve(info.evaluationId);
            if (ep == null)
            {
                return new HttpStatusCodeResult(400, "Invalid request");
            }

            if (ep.SupplementaryInfo == null)
            {
                ep.SupplementaryInfo = new SupplementaryInfo();
            }
            if (ep.SupplementaryInfo.Info == null)
            {
                ep.SupplementaryInfo.Info = new List<info>();
            }

            info.PostedDate = string.Format("{0:dd-MMM-yyyy hh:mm:ss tt}", DateTime.Now);
            info.PosterName = new Utils().GetloggedInProfile().FullName;
            info.id = ep.SupplementaryInfo.Info.Count() + 1;
            ep.SupplementaryInfo.Info.Add(info);

            try
            {
                dao.Update(ep);
                dao.CommitChanges();
                return Json(new { PostedDate = info.PostedDate, id = info.id, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                dao.RollbackChanges();
                Logger.LogError(ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }


        private IList<Organizations> OrgsRepo()
        {
            if ((HttpContext.Session["OrganizationList"] as List<Organizations>) == null)
            {
                HttpContext.Session["OrganizationList"] = new OrganizationDAO().RetrieveAll();
            }
            return HttpContext.Session["OrganizationList"] as List<Organizations>;
        }
    }
}