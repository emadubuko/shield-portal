using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using DQA.DAL.Model;
using ShieldPortal.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize(Roles = "shield_team,sys_admin,ip")]
    public class DQAFY2017Q2Controller : Controller
    {
        public ActionResult Index()
        {
            int ip_id = 0;
            if (User.IsInRole("ip"))
            {
                ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            }
            ViewBag.ip_id = ip_id;

            return View("Dashboard");
        }

        public ActionResult Analytics()
        {
            return View();
        }

        public ActionResult IpDQA()
        {
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                PopulateStates();
                return View("AllIPDQA");
            }

            else if (User.IsInRole("ip"))
            {
                var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
                ViewBag.ip_name = new Services.Utils().GetloggedInProfile().Organization.Name;

                PopulateStates();

                ViewBag.ip_id = ip_id;
                return View();
            }
            return View("~/Views/Shared/Denied.cshtml");
        }


        public ActionResult GetDQA(int id)
        {
            ViewBag.metadataId = id;
            return View();
        }

        public ActionResult IPDQAResult(int id)
        {
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                PopulateStates();
                PopulateStates();

                ViewBag.ip_id = id;
                return View("IpDQAAdmin");
            }
            return View("~/Views/Shared/Denied.cshtml");
        }

        public ActionResult UploadDQA()
        {
            var ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            ViewBag.ip_id = ip_id;

            return View();
        }

        public ActionResult PendingFacilities()
        {
            return View();
        }

        public ActionResult DQAResult()
        {
            int ip_id = 0;
            if (User.IsInRole("ip"))
            {
                ip_id = new Services.Utils().GetloggedInProfile().Organization.Id;
            }
            ViewBag.ip_id = ip_id;
            return View();
        }


        public void PopulateStates(object selectStatus = null)
        {
            var statusQuery = new BaseDAO<State, long>().RetrieveAll();
            ViewBag.states = new SelectList(statusQuery, "state_code", "state_name", selectStatus);
        }

        public ActionResult UploadPivotTable()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<UploadList> previousUploads = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = DQA.DAL.Business.Utility.RetrievePivotTables(0);
            }
            else
            {
                previousUploads = DQA.DAL.Business.Utility.RetrievePivotTables(profile.Organization.Id);
            }
            return View(previousUploads);
        }

        public ActionResult UploadRadet()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            List<RadetUploadReport> previousUploads = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                previousUploads = new RadetUploadReportDAO().RetrieveRadetUpload(0, 2017, "Q2(Jan-Mar)");
                ViewBag.showdelete = true;
            }
            else
            {
                ViewBag.showdelete = false;
                previousUploads = new RadetUploadReportDAO().RetrieveRadetUpload(profile.Organization.Id, 2017, "Q2(Jan-Mar)");
            }
            List<RadetReportModel> list = new List<ViewModel.RadetReportModel>();
            if (previousUploads != null)
            {
                list = (from entry in previousUploads
                        select new RadetReportModel
                        {
                            IP = entry.IP.ShortName,
                            dqa_quarter = entry.dqa_quarter,
                            dqa_year = entry.dqa_year,
                            UploadedBy = entry.UploadedBy.FullName,
                            DateUploaded = entry.DateUploaded,
                            Id = entry.Id,
                            Uploads = (from item in entry.Uploads
                                       select new RadetListing
                                       {
                                           PatientId = item.PatientId,
                                           HospitalNo = item.HospitalNo,
                                           Sex = item.Sex,
                                           Age_at_start_of_ART_in_months = item.Age_at_start_of_ART_in_months,
                                           Age_at_start_of_ART_in_years = item.Age_at_start_of_ART_in_years,
                                           ARTStartDate = item.ARTStartDate,
                                           LastPickupDate = item.LastPickupDate,
                                           MonthsOfARVRefill = item.MonthsOfARVRefill,
                                           RegimenLineAtARTStart = item.RegimenLineAtARTStart,
                                           RegimenAtStartOfART = item.RegimenAtStartOfART,
                                           CurrentRegimenLine = item.CurrentRegimenLine,
                                           CurrentARTRegimen = item.CurrentARTRegimen,
                                           Pregnancy_Status = item.Pregnancy_Status,
                                           Current_Viral_Load = item.Current_Viral_Load,
                                           Date_of_Current_Viral_Load = item.Date_of_Current_Viral_Load,
                                           Viral_Load_Indication = item.Viral_Load_Indication,
                                           CurrentARTStatus = item.CurrentARTStatus,
                                           SelectedForDQA = item.SelectedForDQA,
                                           RadetYear = item.RadetYear
                                       }).ToList()
                        }).ToList();
            }

            //Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(Model))

            return View(list);
        }
    }
}