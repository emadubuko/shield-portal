using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using MPM.DAL;
using MPM.DAL.DAO;
using MPM.DAL.Processor;
using Newtonsoft.Json;
using ShieldPortal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]
    public class MPMController : Controller
    {
        Profile loggedinProfile;

        public MPMController()
        {
            loggedinProfile = new Services.Utils().GetloggedInProfile();
        }

        // GET: MPM
        public ActionResult Index()
        {
            IndexPeriods = new Dictionary<string, int>();
            for (int i = -6; i < 6; i++)
            {
                IndexPeriods.Add(DateTime.Now.AddMonths(i).ToString("MMM-yy"), i + 6);
            }

            var ims = new UploadViewModel
            {
                IPReports = new Dictionary<string, List<string>>()
            };

            var gsm = new UploadViewModel
            {
                IPReports = new Dictionary<string, List<string>>()
            };

            var mpmDAO = new MPMDAO();

            var submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", MPM.DAL.DTO.ReportLevel.State);

            var reports = submissions.GroupBy(x => x.IPName);

            foreach (var iplevel in reports)
            {
                ims.ImplementingPartner = iplevel.Key;
                foreach (var state in iplevel.GroupBy(x => x.ReportingLevelValue))
                {
                    var entries = state.ToList().Select(x => x.ReportPeriod).ToList();
                    List<string> uploaded = new List<string>(12);

                    foreach (var index in IndexPeriods)
                    {

                        if (entries.Any(x => x.Contains(index.Key)))
                        {
                            int id = submissions.FirstOrDefault(c => c.IPName == iplevel.Key
                           && c.ReportingLevelValue == state.Key &&
                           c.ReportPeriod.Contains(index.Key)).Id;

                            uploaded.Add(true + "|" + id);
                        }
                        else
                            uploaded.Add(false + "|");
                    }
                    ims.IPReports.Add(state.Key + "|" + iplevel.Key, uploaded);
                }
            }
            ViewBag.reportPeriod = mpmDAO.GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);
            ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();
            ViewBag.userRole = loggedinProfile.RoleName;
            return View(ims);
        }



        public ActionResult GSMUploadTracker()
        {
            var ims = new UploadViewModel
            {
                IPReports = new Dictionary<string, List<string>>()
            };

            var gsm = new UploadViewModel
            {
                IPReports = new Dictionary<string, List<string>>()
            };

            var mpmDAO = new MPMDAO();

            var submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", MPM.DAL.DTO.ReportLevel.IP);

            var reports = submissions.GroupBy(x => x.IPName);

            foreach (var iplevel in reports)
            {
                ims.ImplementingPartner = iplevel.Key;
                foreach (var state in iplevel.GroupBy(x => x.ReportingLevelValue))
                {
                    var entries = state.ToList().Select(x => x.ReportPeriod).ToList();
                    List<string> uploaded = new List<string>(12);

                    foreach (var index in GSMIndexPeriods)
                    {
                        if (entries.Any(x => x.Contains(index.Key)))
                        {
                            int id = submissions.FirstOrDefault(c => c.IPName == iplevel.Key
                           && c.ReportingLevelValue == state.Key &&
                           c.ReportPeriod.Contains(index.Key)).Id;

                            uploaded.Add(true + "|" + id);
                        }
                        else
                            uploaded.Add(false + "|");
                    }
                    ims.IPReports.Add(state.Key + "|" + iplevel.Key, uploaded);
                }
            }
            ViewBag.reportPeriod = mpmDAO.GetGSMLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);
            ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();
            ViewBag.userRole = loggedinProfile.RoleName;
            return View(ims);
        }

        public ActionResult deleteMPMUpload(int Id)
        {

            var dao = new MPMDAO();
            var previously = dao.Retrieve(Id);
            if (previously != null)
            {
                FileInfo myfileinf = new FileInfo(previously.FilePath);
                myfileinf.Delete();
                SqlCommand command = new SqlCommand();
                command.CommandText = "sp_delete_MPM_Upload";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@metadata_id", Id);

                new MPMDAO().executeDeleteMPM(command);
                //  return previously.FilePath.Split(new string[] { "\\Report\\" }, StringSplitOptions.RemoveEmptyEntries)[1];

            }

            var redirectUrl = new UrlHelper(Request.RequestContext).Action("GSMUploadTracker");
            return Json(new { Url = redirectUrl });
            // return View(GSMUploadTracker());
        }

        public ActionResult deleteMPMUpload_IMS(int Id)
        {

            var dao = new MPMDAO();
            var previously = dao.Retrieve(Id);
            if (previously != null)
            {
                FileInfo myfileinf = new FileInfo(previously.FilePath);
                myfileinf.Delete();
                SqlCommand command = new SqlCommand();
                command.CommandText = "sp_delete_MPM_Upload";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@metadata_id", Id);

                new MPMDAO().executeDeleteMPM(command);
                //  return previously.FilePath.Split(new string[] { "\\Report\\" }, StringSplitOptions.RemoveEmptyEntries)[1];

            }

            var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index");
            return Json(new { Url = redirectUrl });
            // return View(GSMUploadTracker());
        }

        public ActionResult CompletenessReport()
        {
            var cmd = new SqlCommand
            {
                CommandText = "sp_mpm_completeness_report",
                CommandType = CommandType.StoredProcedure
            };
            var dt = new MPMDAO().GetDatable(cmd);
            var report = Utilities.ConvertToList<CompletenessReport>(dt);
            return View();
        }



        public JsonResult CompletenessReportData()
        {
            var cmd = new SqlCommand
            {
                CommandText = "sp_mpm_completeness_report",
                CommandType = CommandType.StoredProcedure
            };
            var dt = new MPMDAO().GetDatable(cmd);
            var report = Utilities.ConvertToList<CompletenessReport>(dt);

            List<dynamic> gsm = new List<dynamic>();
            List<dynamic> ims = new List<dynamic>();
            foreach (var item in report.Where(x => x.GSM_2).GroupBy(x => x.ReportingPeriod))
            {
                string key = item.Key;
                int number = item.Count();
                double perc = Math.Round(100 * 1.0 * item.Count() / 20, 2);
                double per2 = Math.Round(100 * 1.0 * (item.Count() / 20), 2);
                gsm.Add(new
                {
                    ReportingPeriod = item.Key,
                    percent = Math.Round(100 * 1.0 * item.Count() / 300, 0)
                    //   percent = Math.Round(100 * 1.0 * item.Count() / 20, 0)
                });
            }
            foreach (var item in report.Where(x => x.GranularSite && !x.GSM_2).GroupBy(x => x.ReportingPeriod))
            {
                ims.Add(new
                {
                    ReportingPeriod = item.Key,
                    percent = Math.Round(100 * 1.0 * item.Count() / 155, 0)
                });
            }

            List<dynamic> ims_fac_ind_comp = new List<dynamic>();
            foreach (var item in report.Where(x => x.GranularSite && !x.GSM_2).GroupBy(x => x.ReportingPeriod))
            {
                //14 indicators
                int no_of_facilities = item.DistinctBy(x => x.facilityId).DistinctBy(x => x.indicator).Count();
                ims_fac_ind_comp.Add(new
                {
                    ReportingPeriod = item.Key,
                    percent = Math.Round(100 * 1.0 * no_of_facilities / 155, 0)
                });
            }

            List<dynamic> gsm_fac_ind_comp = new List<dynamic>();
            foreach (var item in report.Where(x => x.GSM_2).GroupBy(x => x.ReportingPeriod))
            {
                //14 indicators
                int no_of_facilities = item.DistinctBy(x => x.facilityId).DistinctBy(x => x.indicator).Count();
                gsm_fac_ind_comp.Add(new
                {
                    ReportingPeriod = item.Key,
                    percent = Math.Round(100 * 1.0 * no_of_facilities / 20, 0)
                });
            }


            ////last report period
            //string reportPeriod = new MPMDAO().GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);
            //List<dynamic> gsm_timeliness = new List<dynamic>();
            //foreach (var item in report.Where(x => x.GSM_2).GroupBy(x => x.ReportingPeriod))
            //{
            //    //14 indicators
            //    int no_of_facilities = item.DistinctBy(x => x.FacilityId).DistinctBy(x => x.Indicator).Count();
            //    gsm_timeliness.Add(new
            //    {
            //        ReportingPeriod = item.Key,
            //        percent = Math.Round(100 * 1.0 * no_of_facilities / 155, 0)
            //    });
            //}

            return Json(new
            {
                gsm,
                ims,
                ims_fac_ind_comp,
                gsm_fac_ind_comp
            });
        }


        [HttpPost]
        public string DownloadPreviousReport(int Id)//(string IPState)
        {
            var dao = new MPMDAO();
            var previously = dao.Retrieve(Id);
            if (previously != null)
            {
                return previously.FilePath.Split(new string[] { "\\Report\\" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            else
                return "";
            //string IP = IPState.Split('|')[1];
            //string State = IPState.Split('|')[0];
            //var ip = new OrganizationDAO().SearchByShortName(IP);
            //string period = "";// "Jul-18";
            //var dao = new MPMDAO();
            //var previously = dao.GenerateIPUploadReports(ip.Id, period, State, MPM.DAL.DTO.ReportLevel.State);

            //if (previously != null && previously.FirstOrDefault() != null)
            //{
            //    return previously.FirstOrDefault().FilePath.Split(new string[] { "\\Report\\" }, StringSplitOptions.RemoveEmptyEntries)[1];
            //}
            //return "";
        }

        public JsonResult ReportDetails(string reportPeriod)
        {
            var mpmDAO = new MPMDAO();
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;
            }

            if (string.IsNullOrEmpty(reportPeriod))
                reportPeriod = mpmDAO.GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);

            DataTable reports = null;

            try
            {
                reports = mpmDAO.GetUploadReport(reportPeriod);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }


            List<dynamic> reportView = new List<dynamic>();

            foreach (DataRow dr in reports.Rows)
            {
                if (!string.IsNullOrEmpty(ip) && dr.Field<string>("IP") != ip)
                    continue;
                reportView.Add(new
                {
                    IP = dr.Field<string>("IP"),
                    State = dr.Field<string>("State"),
                    LGA = dr.Field<string>("LGA"),
                    Facility = dr.Field<string>("Facility"),

                    PMTCT = dr.Field<string>("PMTCT"),
                    PMTCT_EID = dr.Field<string>("PMTCT_EID"),
                    ART = dr.Field<string>("ART"),
                    HTS = dr.Field<string>("HTS"),
                    HTS_PITC = dr.Field<string>("HTS_PITC"),
                    LinkageToTx = dr.Field<string>("Linkage_To_Treatment"),
                    PMTCT_Viral_Load = dr.Field<string>("PMTCT_Viral_Load"),
                    TB_Screening = dr.Field<string>("TB_Screening"),
                    TB_Presumptive = dr.Field<string>("TB_Presumptive"),
                    TB_Bacteriology_Diagnosis = dr.Field<string>("TB_Bacteriology_Diagnosis"),
                    TB_Diagnosed = dr.Field<string>("TB_Diagnosed"),
                    TB_Treatment = dr.Field<string>("TB_Treatment"),
                    TPT_Eligible = dr.Field<string>("TPT_Eligible"),
                    TB_ART = dr.Field<string>("TB_ART"),
                });
            }
            //ViewBag.CompletionReport = reportView;
            return Json(reportView);
        }

        public ActionResult Upload()
        {
            return View();
        }

        public async Task<ActionResult> NDR_Statictics()
        {
            var data = await new NDR_StatisticsDAO().RetrieveAll();
            string reportPeriod = new MPMDAO().GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;

                data = data.Where(x => x.Facility.Organization.Id == profile.Organization.Id).ToList();
            }

            data = data//.Where(x => x.ReportPeriod == reportPeriod)
                .OrderByDescending(o => o.Facility.GranularSite).ToList();

            ViewBag.reportPeriod = reportPeriod;
            return View(data);
        }


        public JsonResult ProcessFile()
        {
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Uploads/MPM/" + loggedinProfile.Organization.ShortName) + "\\";
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }


            if (Request.Files.Count == 0)
                return Json("<span style='color: red;'> No File Uploaded</span>");

            var filePath = directory + DateTime.Now.ToString("dd MMM yyyy") + "_" + Path.GetFileName(Request.Files[0].FileName);

            try
            {
                Request.Files[0].SaveAs(filePath);
            }
            catch (Exception e)
            {

            }


            try
            {
                new TemplateProcessor().ReadFile(Request.Files[0].InputStream, loggedinProfile, filePath);
                return Json("Uploaded Succesfully");
            }
            catch (Exception ex)
            {
                return Json("<span style='color: red;'>" + ex.Message + "</span>");
            }
        }

        //download for IMS monthly template
        public ActionResult Download()
        {
            var facilities = new HealthFacilityDAO().RetrievebyIP(loggedinProfile.Organization.Id);
            List<State> states = new List<State>();
            if (facilities != null)
                states.AddRange(facilities.Select(x => x.LGA.State).Distinct());

            return View(states);
        }

        //download for GSM bi-weekly template
        public async Task<ActionResult> DownloadTemplateForGSM()
        {
            var file = new TemplateProcessor().PopulateGSMTemplate(loggedinProfile);
            using (var stream = System.IO.File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                byte[] fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file.Name);
            }
        }

        //download for IMS monthly template
        [HttpPost]
        public JsonResult DownloadTemplate(string state)
        {
            string file = new TemplateProcessor().PopulateTemplate(loggedinProfile, state);
            return Json(file, JsonRequestBehavior.AllowGet);
            //using (var stream = System.IO.File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //{
            //    byte[] fileBytes = new byte[stream.Length];
            //    await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
            //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "CDC MPM Tool_" + loggedinProfile.Organization.ShortName + "_" + state + ".xlsm");
            //} 
        }

        public ActionResult Dashboard(string lastreportedPeriod = "")
        {
            var IPLocation = new List<IPLGAFacility>();
            IList<HealthFacility> facilities;
            IList<IPUploadReport> submissions;

            var mpmDAO = new MPMDAO();
            DataTable reports = null;

            try
            {
                submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", null);
                ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();


                if (User.IsInRole("ip"))
                {
                    submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.Organization.Id, "", "", null);
                    facilities = new HealthFacilityDAO().RetrievebyIP(loggedinProfile.Organization.Id);
                }
                else
                {
                    submissions = mpmDAO.GenerateIPUploadReports(0, "", "", null);
                    facilities = new HealthFacilityDAO().RetrieveAll();
                }

                if (string.IsNullOrEmpty(lastreportedPeriod))
                    lastreportedPeriod = new MPMDAO().GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);


                reports = mpmDAO.GetUploadReport(lastreportedPeriod);
                List<dynamic> reportView = new List<dynamic>();

                foreach (DataRow dr in reports.Rows)
                {
                    Int64 facilityId = dr.Field<Int64>("facilityId");
                    var hf = facilities.FirstOrDefault(x => x.Id == facilityId);

                    if (hf != null)
                    {
                        IPLocation.Add(
                        new IPLGAFacility
                        {
                            FacilityName = hf.Name,
                            IP = hf.Organization.ShortName,
                            LGA = hf.LGA,
                        });
                    }
                }

                ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();
                ViewBag.LastReportedPeriod = lastreportedPeriod;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }

            return View("iDashboard", IPLocation);


            // return View();
        }

        public ActionResult ReportingRate(string lastreportedPeriod = "")
        {
            var IPLocation = new List<IPLGAFacility>();
            IList<HealthFacility> facilities;
            IList<IPUploadReport> submissions;

            var mpmDAO = new MPMDAO();
            DataTable reports = null;

            try
            {
                submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", null);
                ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();


                if (User.IsInRole("ip"))
                {
                    submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.Organization.Id, "", "", null);
                    facilities = new HealthFacilityDAO().RetrievebyIP(loggedinProfile.Organization.Id);
                }
                else
                {
                    submissions = mpmDAO.GenerateIPUploadReports(0, "", "", null);
                    facilities = new HealthFacilityDAO().RetrieveAll();
                }

                if (string.IsNullOrEmpty(lastreportedPeriod))
                    lastreportedPeriod = new MPMDAO().GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);


                reports = mpmDAO.GetUploadReport(lastreportedPeriod);
                List<dynamic> reportView = new List<dynamic>();

                foreach (DataRow dr in reports.Rows)
                {
                    Int64 facilityId = dr.Field<Int64>("facilityId");
                    var hf = facilities.FirstOrDefault(x => x.Id == facilityId);

                    if (hf != null)
                    {
                        IPLocation.Add(
                        new IPLGAFacility
                        {
                            FacilityName = hf.Name,
                            IP = hf.Organization.ShortName,
                            LGA = hf.LGA,
                        });
                    }
                }

                ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();
                ViewBag.LastReportedPeriod = lastreportedPeriod;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }

            return View("iReportingRate", IPLocation);


            // return View();
        }


        public ActionResult CompletenessReports(string lastreportedPeriod = "")
        {
            var IPLocation = new List<IPLGAFacility>();
            IList<HealthFacility> facilities;
            IList<IPUploadReport> submissions;

            var mpmDAO = new MPMDAO();
            DataTable reports = null;

            try
            {
                submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", null);
                ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();


                if (User.IsInRole("ip"))
                {
                    submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.Organization.Id, "", "", null);
                    facilities = new HealthFacilityDAO().RetrievebyIP(loggedinProfile.Organization.Id);
                }
                else
                {
                    submissions = mpmDAO.GenerateIPUploadReports(0, "", "", null);
                    facilities = new HealthFacilityDAO().RetrieveAll();
                }

                if (string.IsNullOrEmpty(lastreportedPeriod))
                    lastreportedPeriod = new MPMDAO().GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);


                reports = mpmDAO.GetUploadReport(lastreportedPeriod);
                List<dynamic> reportView = new List<dynamic>();

                foreach (DataRow dr in reports.Rows)
                {
                    Int64 facilityId = dr.Field<Int64>("facilityId");
                    var hf = facilities.FirstOrDefault(x => x.Id == facilityId);

                    if (hf != null)
                    {
                        IPLocation.Add(
                        new IPLGAFacility
                        {
                            FacilityName = hf.Name,
                            IP = hf.Organization.ShortName,
                            LGA = hf.LGA,
                        });
                    }
                }

                ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();
                ViewBag.LastReportedPeriod = lastreportedPeriod;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw ex;
            }

            return View("iCompletenessReports", IPLocation);


            // return View();
        }


        [HttpPost]
        public dynamic RetriveData(MPMDataSearchModel searchModel = null)
        {
            Dictionary<string, string> storedProcedures = new Dictionary<string, string>
            {
                { "hts_index_testing" ,"sp_mpm_hts_index" },
                { "hts_other_pitc", "sp_MPM_HTS_Other_PITC" },
                {"ART","sp_mpm_ART" },
                {"Linkage","sp_mpm_LinkageToTreatment" },
                {"PMTCT_VL","sp_mpm_PMTCT_Viral_Load" },
                {"PMTCT","sp_mpm_PMTCT" },
                { "Tb_Stat", "[sp_mpm_TB_Stat]" },
                {"TB_treatment","[sp_mpm_TB_treatment]" },
                {"tb_tpt","[sp_mpm_TB_TPT]" }
            };

            MPMDAO dao = new MPMDAO();
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;

                //make sure it is still the IP
                searchModel.IPs = new List<string> { ip };
            }

            Dictionary<string, dynamic> _data = new Dictionary<string, dynamic>();
            foreach (var sp in storedProcedures)
            {
                var data = dao.RetriveDataAsDataTables(sp.Value, searchModel);
                if (sp.Key == "hts_index_testing")
                {
                    _data.Add(sp.Key, DrillDownBubbleData(data));
                }
                if (sp.Key == "hts_other_pitc")
                {
                    _data.Add(sp.Key, GenerateHTSOtherPITC(data));
                }
                if (sp.Key == "ART")
                {
                    _data.Add(sp.Key, GenerateTx_Ret_And_Viral_Load(data));
                }
                if (sp.Key == "Linkage")
                {
                    _data.Add(sp.Key, GenerateLinkageData(data));
                }
                if (sp.Key == "PMTCT_VL")
                {
                    _data.Add(sp.Key, GeneratePMTCT_VL(data));
                }
                if (sp.Key == "PMTCT")
                {
                    _data.Add(sp.Key, GeneratePMTCT_Cascade(data));
                }
                if (sp.Key == "Tb_Stat")
                {
                    _data.Add(sp.Key, GenerateTB_STAT(data));
                }//
                if (sp.Key == "TB_treatment")
                {
                    _data.Add(sp.Key, GenerateTB_Treatment(data));
                }
                if (sp.Key == "tb_tpt")
                {
                    _data.Add(sp.Key, GenerateTB_TPT(data));
                }
            }

            return JsonConvert.SerializeObject(new
            {
                _data
            });
        }


        [HttpPost]
        public dynamic RetriveCompletenessData(MPMDataSearchModel searchModel = null)
        {
            Dictionary<string, string> storedProcedures = new Dictionary<string, string>
            {

                {"Comp_Stat", "sp_mpm_HTS_Index_Completeness_Rate" },
                {"Comp_Stat_HTS_TST", "sp_mpm_HTS_TST_Completeness_Rate" },
                {"Comp_Stat_HTS_Other_PITC", "sp_MPM_HTS_Other_PITC_Completeness_Rate" },
                {"Comp_Stat_ART", "sp_mpm_ART_Completeness_Rate" },
                {"Comp_Stat_PMTCT", "sp_mpm_PMTCT_Completeness_Rate" },
                {"Comp_Stat_PMTCT_EID", "sp_mpm_PMTCT_EID_Completeness_Rate"},
                {"Comp_Stat_PMTCT_Viral_Load", "sp_mpm_PMTCT_Viral_Load_Completeness_Report"},

                  {"Comp_TB_screened", "[sp_mpm_TB_Screened_Completeness_Rate]" },
                  {"Comp_TB_Presumptive", "[sp_mpm_TB_Presumptive_Completeness_Rate]" },
                  {"Comp_TB_Bacteriology", "[sp_mpm_TB_Bacteriology_Diagnosis_Completeness_Rate]" },
                  {"Comp_TB_Diagnosis", "[sp_mpm_TB_Diagnosis_Completeness_Rate]" },
                  {"Comp_TB_Treatment", "[sp_mpm_TB_Treatment_Started_Completeness_Rate]" },
                  {"Comp_TB_TBT", "[sp_mpm_TB_TBT_Eligible_Completeness_Rate]" },
                  {"Comp_TB_Relapsed_Status", "[sp_mpm_TB_New_Replapsed_Known_Status_Completeness_Rate]" },
                  {"Comp_TB_Relapsed_POS", "[sp_mpm_TB_New_Replapsed_Known_Pos_Completeness_Rate]" },
                  {"Comp_TB_Relapsed", "[sp_mpm_TB_New_Replapsed_Completeness_Rate]" },
                  {"Comp_TB_ART", "[sp_mpm_TB_ART_Completeness_Rate]" }



            };

            MPMDAO dao = new MPMDAO();
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;

                //make sure it is still the IP
                searchModel.IPs = new List<string> { ip };
            }

            Dictionary<string, dynamic> _data = new Dictionary<string, dynamic>();
            foreach (var sp in storedProcedures)
            {
                var data = dao.RetriveDataAsDataTablesCompleteness(sp.Value, searchModel);

                if (sp.Key == "Comp_Stat")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_HTS_Index(data));
                }
                if (sp.Key == "Comp_Stat_HTS_TST")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_HTS_TST(data));
                }

                if (sp.Key == "Comp_Stat_HTS_Other_PITC")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_HTS_Other_PITC(data));
                }

                if (sp.Key == "Comp_Stat_ART")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_ART(data));
                }

                if (sp.Key == "Comp_Stat_PMTCT")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT(data));
                }

                if (sp.Key == "Comp_Stat_PMTCT_EID")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_EID(data));
                }
                if (sp.Key == "Comp_Stat_PMTCT_Viral_Load")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }


                if (sp.Key == "Comp_TB_screened")
                {
                    _data.Add(sp.Key, GenerateCOMPLETENESSRATE_TB_Screened(data));
                }
                if (sp.Key == "Comp_TB_Presumptive")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_Bacteriology")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_Diagnosis")
                {
                 //   _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_Treatment")
                {
                   // _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_TBT")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_Relapsed_Status")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_Relapsed_POS")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }
                if (sp.Key == "Comp_TB_Relapsed")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }

                if (sp.Key == "Comp_TB_ART")
                {
                  //  _data.Add(sp.Key, GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(data));
                }

            }

            return JsonConvert.SerializeObject(new
            {
                _data
            });
        }

        [HttpPost]
        public dynamic RetriveReportingRateData(MPMDataSearchModel searchModel = null)
        {
            Dictionary<string, string> storedProcedures = new Dictionary<string, string>
            {

                 { "Comp_Stat", "sp_mpm_completeness_report" },
           
            };

            MPMDAO dao = new MPMDAO();
            string ip = "";
            if (User.IsInRole("ip"))
            {
                var profile = new Services.Utils().GetloggedInProfile();
                ip = profile.Organization.ShortName;

                //make sure it is still the IP
                searchModel.IPs = new List<string> { ip };
            }

            Dictionary<string, dynamic> _data = new Dictionary<string, dynamic>();
            foreach (var sp in storedProcedures)
            {
                var data = dao.RetriveDataAsDataTablesCompleteness(sp.Value, searchModel);

                if (sp.Key == "Comp_Stat")
                {
                    _data.Add(sp.Key, GenerateREPORTINGRATE(data));
                }
             

            }

            return JsonConvert.SerializeObject(new
            {
                _data
            });
        }

        //tb_tpt
        public dynamic GenerateTB_TPT(DataTable dt)
        {
            List<TB_TPT_ViewModel> lst = Utilities.ConvertToList<TB_TPT_ViewModel>(dt);
            var groupedData = lst.GroupBy(x => x.State);

            var state_eligible = new List<dynamic>();
            var state_started = new List<dynamic>();
            var state_percnt = new List<dynamic>();

            var lga_drill_down = new List<dynamic>();

            foreach (var state in groupedData)
            {
                var eligible = state.Sum(x => x.PLHIV_eligible_for_TPT);
                var started = state.Sum(x => x.Started_on_TPT);
                double _percnt = 0;
                if (eligible != 0)
                    _percnt = Math.Round(100 * 1.0 * started / eligible, 0);

                state_eligible.Add(new
                {
                    y = eligible,
                    name = state.Key,
                    drilldown = state.Key
                });
                state_started.Add(new
                {
                    y = started,
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                state_percnt.Add(new
                {
                    y = _percnt,
                    name = state.Key,
                    drilldown = state.Key + "  "
                });

                var lga_eligible = new List<dynamic>();
                var lga_started = new List<dynamic>();
                var lga_percnt = new List<dynamic>();

                var lgas = new List<string>();
                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    lgas.Add(lga.Key);

                    var eligible_l = lga.Sum(x => x.PLHIV_eligible_for_TPT);
                    var started_l = lga.Sum(x => x.Started_on_TPT);
                    double _percnt_l = 0;
                    if (eligible_l != 0)
                        _percnt_l = Math.Round(100 * 1.0 * started_l / eligible_l, 0);

                    lga_eligible.Add(new
                    {
                        y = eligible_l,
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_started.Add(new
                    {
                        y = started_l,
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_percnt.Add(new
                    {
                        y = _percnt_l,
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });


                    var facility_eligible = new List<dynamic>();
                    var facility_started = new List<dynamic>();
                    var facility_percnt = new List<dynamic>();

                    var facilities = new List<string>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facilities.Add(fty.Key);

                        var eligible_f = fty.Sum(x => x.PLHIV_eligible_for_TPT);
                        var started_f = fty.Sum(x => x.Started_on_TPT);
                        double _percnt_f = 0;
                        if (eligible_f != 0)
                            _percnt_f = Math.Round(100 * 1.0 * started_f / eligible_f, 0);

                        facility_eligible.Add(new List<dynamic>
                        {
                            fty.Key,
                           eligible_f
                        });
                        facility_started.Add(new List<dynamic>
                        {
                            fty.Key,
                            started_f
                        });
                        facility_percnt.Add(new List<dynamic>
                        {
                            fty.Key,
                            _percnt_f,
                        });
                    }

                    //add facility
                    lga_drill_down.Add(new { name = "PLHIV Eligible", id = lga.Key, data = facility_eligible, type = "column", categories = facilities });
                    lga_drill_down.Add(new { name = "Started TPT", id = lga.Key + " ", data = facility_started, type = "column", categories = facilities });
                    lga_drill_down.Add(new { name = "% TPT", yAxis = 1, type = "scatter", id = lga.Key + "  ", data = facility_percnt, categories = facilities });
                }
                //add lga
                lga_drill_down.Add(new { name = "New TB Cases", id = state.Key, data = lga_eligible, categories = lgas, type = "column" });
                lga_drill_down.Add(new { name = "Started TPT", id = state.Key + " ", data = lga_started, categories = lgas, type = "column" });
                lga_drill_down.Add(new { name = "% TPT", yAxis = 1, type = "scatter", id = state.Key + "  ", data = lga_percnt, categories = lgas });

            }

            List<dynamic> state_data = new List<dynamic>
            {
                new
                {
                    name = "PLHIV Eligible",
                    type  = "column",
                    data = state_eligible,
                },
                new
                {
                    name = "Started TPT",
                    type  = "column",
                    data = state_started
                },
                new
                {
                    name = "% TPT",
                    data = state_percnt,
                    yAxis = 1,
                    type = "scatter",
                },
            };

            return new
            {
                state_data,
                lga_drill_down,
                states = groupedData.Select(x => x.Key)
            };

            /*
            var _data = new List<dynamic>();
            foreach (var state in groupedData)
            {
                var started = state.Sum(x => x.Started_on_TPT);
                var eligible = state.Sum(x => x.PLHIV_eligible_for_TPT);
                double _percnt = 0;
                if (eligible != 0)
                    _percnt = Math.Round(100 * 1.0 * started / eligible, 0);

                _data.Add(new
                {
                    started,
                    eligible,
                    _percnt,
                    State = state.Key
                });
            }
            _data = _data.OrderByDescending(t => t.eligible).ToList();

            return new
            {
                started = _data.Select(s => s.started),
                eligible = _data.Select(s => s.eligible),
                Percent = _data.Select(y => y._percnt),
                State = _data.Select(s => s.State),
            };
            */
        }


        //tb_treatment
        public dynamic GenerateTB_Treatment(DataTable dt)
        {
            List<TB_Treatment_ViewModel> lst = Utilities.ConvertToList<TB_Treatment_ViewModel>(dt);

            var groupedData = lst.OrderByDescending(t => t.New_Cases)
                .GroupBy(x => x.State);

            var state_newCases = new List<dynamic>();
            var state_tx_tb = new List<dynamic>();
            var state_tx_tb_percnt = new List<dynamic>();

            var lga_drill_down = new List<dynamic>();

            foreach (var state in groupedData)
            {
                var newcases = state.Sum(x => x.New_Cases);
                var tx_tb = state.Sum(x => x.TX_TB);
                double tb_tx_percnt = 0;
                if (newcases != 0)
                    tb_tx_percnt = Math.Round(100 * 1.0 * tx_tb / newcases, 0);

                state_newCases.Add(new
                {
                    y = newcases,
                    name = state.Key,
                    drilldown = state.Key
                });
                state_tx_tb.Add(new
                {
                    y = tx_tb,
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                state_tx_tb_percnt.Add(new
                {
                    y = tb_tx_percnt,
                    name = state.Key,
                    drilldown = state.Key + "  "
                });

                var lga_newCase = new List<dynamic>();
                var lga_tx_tb = new List<dynamic>();
                var lga_tx_tb_percnt = new List<dynamic>();

                var lgas = new List<string>();
                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    lgas.Add(lga.Key);

                    var newcases_l = lga.Sum(x => x.New_Cases);
                    var tx_tb_l = lga.Sum(x => x.TX_TB);
                    double tb_tx_percnt_l = 0;
                    if (newcases_l != 0)
                        tb_tx_percnt_l = Math.Round(100 * 1.0 * tx_tb_l / newcases_l, 0);

                    lga_newCase.Add(new
                    {
                        y = newcases_l,
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_tx_tb.Add(new
                    {
                        y = tx_tb_l,
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_tx_tb_percnt.Add(new
                    {
                        y = tb_tx_percnt_l,
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });


                    var facility_newCases = new List<dynamic>();
                    var facility_tx_tb = new List<dynamic>();
                    var facility_tx_tb_percnt = new List<dynamic>();

                    var facilities = new List<string>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facilities.Add(fty.Key);

                        var newcases_f = lga.Sum(x => x.New_Cases);
                        var tx_tb_f = lga.Sum(x => x.TX_TB);
                        double tb_tx_percnt_f = 0;
                        if (newcases_f != 0)
                            tb_tx_percnt_f = Math.Round(100 * 1.0 * tx_tb_f / newcases_f, 0);

                        facility_newCases.Add(new List<dynamic>
                        {
                            fty.Key,
                           newcases_f
                        });
                        facility_tx_tb.Add(new List<dynamic>
                        {
                            fty.Key,
                            tx_tb_f
                        });
                        facility_tx_tb_percnt.Add(new List<dynamic>
                        {
                            fty.Key,
                            tb_tx_percnt_f,
                        });
                    }

                    //add facility
                    lga_drill_down.Add(new { name = "New TB Cases", id = lga.Key, data = facility_newCases, type = "column", categories = facilities });
                    lga_drill_down.Add(new { name = "Tx_TB", id = lga.Key + " ", data = facility_tx_tb, type = "column", categories = facilities });
                    lga_drill_down.Add(new { name = "Tx_TB (%)", yAxis = 1, type = "scatter", id = lga.Key + "  ", data = facility_tx_tb_percnt, categories = facilities });
                }
                //add lga
                lga_drill_down.Add(new { name = "New TB Cases", id = state.Key, data = lga_newCase, categories = lgas, type = "column" });
                lga_drill_down.Add(new { name = "Tx_TB", id = state.Key + " ", data = lga_tx_tb, categories = lgas, type = "column" });
                lga_drill_down.Add(new { name = "Tx_TB (%)", yAxis = 1, type = "scatter", id = state.Key + "  ", data = lga_tx_tb_percnt, categories = lgas });

            }

            List<dynamic> state_data = new List<dynamic>
            {
                new
                {
                    name = "New TB Cases",
                    type  = "column",
                    data = state_newCases,
                },
                new
                {
                    name = "Tx_TB",
                    type  = "column",
                    data = state_tx_tb
                },
                new
                {
                    name = "Tx_TB (%)",
                    data = state_tx_tb_percnt,
                    yAxis = 1,
                    type = "scatter",
                },
            };

            return new
            {
                state_data,
                lga_drill_down,
                states = groupedData.Select(x => x.Key)
            };


            /*
            var _data = new List<dynamic>();
            foreach (var state in groupedData)
            {
                var newcases = state.Sum(x => x.New_Cases);
                var tx = state.Sum(x => x.TX_TB);
                double tb_tx_percnt = 0;
                if (newcases != 0)
                    tb_tx_percnt = Math.Round(100 * 1.0 * tx / newcases, 0);

                _data.Add(new
                {
                    Tx_TB = tx,
                    NewCases = newcases,
                    TB_TX_percnt = tb_tx_percnt,
                    State = state.Key
                });
            }
            _data = _data.OrderByDescending(t => t.NewCases).ToList();

            return new
            {
                Tx_TB = _data.Select(s => s.Tx_TB),
                TB_TX_percnt = _data.Select(s => s.TB_TX_percnt),
                NewCases = _data.Select(y => y.NewCases),
                State = _data.Select(s => s.State),
            };
            */
        }

        //tb_stat
        public dynamic GenerateTB_STAT(DataTable dt)
        {
            List<TB_STAT_ViewModel> lst = Utilities.ConvertToList<TB_STAT_ViewModel>(dt);

            var groupedData = lst.OrderBy(x => x.State).GroupBy(x => x.State);

            var tb_screened_state = new List<dynamic>();
            var tb_presumptive_state = new List<dynamic>();
            var tb_bac_diagnosis_state = new List<dynamic>();
            var tb_diagnosed_state = new List<dynamic>();
            var lga_drill_down = new List<dynamic>();

            foreach (var state in groupedData)
            {
                tb_screened_state.Add(new
                {
                    y = state.Sum(x => x.TB_Screened),
                    name = state.Key,
                    drilldown = state.Key
                });
                tb_presumptive_state.Add(new
                {
                    y = state.Sum(x => x.TB_Presumptive),
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                tb_bac_diagnosis_state.Add(new
                {
                    y = state.Sum(x => x.TB_Bacteriology_Diagnosis),
                    name = state.Key,
                    drilldown = state.Key + "  "
                });
                tb_diagnosed_state.Add(new
                {
                    y = state.Sum(x => x.TB_Diagnosed),
                    name = state.Key,
                    drilldown = state.Key + "   "
                });

                var lga_tb_screened = new List<dynamic>();
                var lga_tb_presumptive = new List<dynamic>();
                var lga_tb_bac_diagnosis = new List<dynamic>();
                var lga_tb_diagnosed = new List<dynamic>();

                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    //lga_maternal_clinical_cascade
                    lga_tb_screened.Add(new
                    {
                        y = lga.Sum(x => x.TB_Screened),
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_tb_presumptive.Add(new
                    {
                        y = lga.Sum(x => x.TB_Presumptive),
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_tb_bac_diagnosis.Add(new
                    {
                        y = lga.Sum(x => x.TB_Bacteriology_Diagnosis),
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });
                    lga_tb_diagnosed.Add(new
                    {
                        y = lga.Sum(x => x.TB_Diagnosed),
                        name = lga.Key,
                        drilldown = lga.Key + "   "
                    });

                    var facility_tb_screened = new List<dynamic>();
                    var facility_tb_presumptive = new List<dynamic>();
                    var facility_tb_bac_diagnosis = new List<dynamic>();
                    var facility_tb_diagnosed = new List<dynamic>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facility_tb_screened.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Sum(x => x.TB_Screened),
                        });
                        facility_tb_presumptive.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Sum(x => x.TB_Presumptive)
                        });
                        facility_tb_bac_diagnosis.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Sum(x => x.TB_Bacteriology_Diagnosis),
                        });
                        facility_tb_diagnosed.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Sum(x => x.TB_Diagnosed),
                        });
                    }

                    //add facility
                    lga_drill_down.Add(new { name = "Screened", id = lga.Key, data = facility_tb_screened });
                    lga_drill_down.Add(new { name = "TB presumptive", id = lga.Key + " ", data = facility_tb_presumptive });
                    lga_drill_down.Add(new { name = "Bacteriologic diagnosis", id = lga.Key + "  ", data = facility_tb_bac_diagnosis });
                    lga_drill_down.Add(new { name = "Diagnosed of Active TB", id = lga.Key + "   ", data = facility_tb_diagnosed });
                }
                //add lga
                lga_drill_down.Add(new { name = "Screened", id = state.Key, data = lga_tb_screened });
                lga_drill_down.Add(new { name = "TB presumptive", id = state.Key + " ", data = lga_tb_presumptive });
                lga_drill_down.Add(new { name = "Bacteriologic diagnosis", id = state.Key + "  ", data = lga_tb_bac_diagnosis });
                lga_drill_down.Add(new { name = "Diagnosed of Active TB", id = state.Key + "   ", data = lga_tb_diagnosed });
            }

            List<dynamic> tb_stat = new List<dynamic>
            {
                new
                {
                    name = "Screened",
                    data = tb_screened_state
                },
                new
                {
                    name = "TB presumptive",
                    data = tb_presumptive_state
                },
                new
                {
                    name = "Bacteriologic diagnosis",
                    data = tb_bac_diagnosis_state
                },
                 new
                {
                    name = "Diagnosed of Active TB",
                    data = tb_diagnosed_state
                }
            };

            return new
            {
                tb_stat,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESS_STAT(DataTable dt)
        {
            List<CompletenessReport> lst = Utilities.ConvertToList<CompletenessReport>(dt);

            var groupedData = lst.GroupBy(x => x.State);
            int submitted_gsm = 0;

            int PMTCT = 0;
            int PMTCT_EID = 0;
            int HTS_PITC = 0;
            int HTS = 0;
            int Linkage_To_Treatment = 0;
            int ART = 0;
            int PMTCT_VIRAL_Load = 0;
            int TB_Screening = 0;
            int TB_Presumptive = 0;
            int TB_Bacteriology_Diagnosis = 0;
            int TB_Diagnosed = 0;
            int TB_Treatment = 0;
            int TPT_Eligible = 0;
            int TB_ART = 0;

            int IMS_PMTCT = 0;
            int IMS_PMTCT_EID = 0;
            int IMS_HTS_PITC = 0;
            int IMS_HTS = 0;
            int IMS_Linkage_To_Treatment = 0;
            int IMS_ART = 0;
            int IMS_PMTCT_VIRAL_Load = 0;
            int IMS_TB_Screening = 0;
            int IMS_TB_Presumptive = 0;
            int IMS_TB_Bacteriology_Diagnosis = 0;
            int IMS_TB_Diagnosed = 0;
            int IMS_TB_Treatment = 0;
            int IMS_TPT_Eligible = 0;
            int IMS_TB_ART = 0;

            var lga_drill_down = new List<dynamic>();

            var state_gsm = new List<dynamic>();
            var state_ims = new List<dynamic>();

            foreach (var state in groupedData)
            {
                PMTCT = 0;
                PMTCT_EID = 0;
                HTS_PITC = 0;
                HTS = 0;
                Linkage_To_Treatment = 0;
                ART = 0;
                PMTCT_VIRAL_Load = 0;
                TB_Screening = 0;
                TB_Presumptive = 0;
                TB_Bacteriology_Diagnosis = 0;
                TB_Diagnosed = 0;
                TB_Treatment = 0;
                TPT_Eligible = 0;
                TB_ART = 0;

                IMS_PMTCT = 0;
                IMS_PMTCT_EID = 0;
                IMS_HTS_PITC = 0;
                IMS_HTS = 0;
                IMS_Linkage_To_Treatment = 0;
                IMS_ART = 0;
                IMS_PMTCT_VIRAL_Load = 0;
                IMS_TB_Screening = 0;
                IMS_TB_Presumptive = 0;
                IMS_TB_Bacteriology_Diagnosis = 0;
                IMS_TB_Diagnosed = 0;
                IMS_TB_Treatment = 0;
                IMS_TPT_Eligible = 0;
                IMS_TB_ART = 0;


                PMTCT = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT").Count();
                PMTCT_EID = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT_EID").Count();
                ART = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "ART").Count();
                HTS = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "HTS").Count();
                HTS_PITC = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "HTS_PITC").Count();
                Linkage_To_Treatment = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "Linkage_To_Treatment").Count();
                PMTCT_VIRAL_Load = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT_VIRAL_Load").Count();
                TB_Screening = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TB_Screening").Count();
                TB_Presumptive = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TB_Presumptive").Count();
                TB_Bacteriology_Diagnosis = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TB_Bacteriology_Diagnosis").Count();
                TB_Diagnosed = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TB_Diagnosed").Count();
                TB_Treatment = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TB_Treatment").Count();
                TPT_Eligible = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TPT_Eligible").Count();
                TB_ART = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "TB_ART").Count();



                IMS_PMTCT = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT").Count();
                IMS_PMTCT_EID = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT_EID").Count();
                IMS_ART = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "ART").Count();
                IMS_HTS = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "HTS").Count();
                IMS_HTS_PITC = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "HTS_PITC").Count();
                IMS_Linkage_To_Treatment = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "Linkage_To_Treatment").Count();
                IMS_PMTCT_VIRAL_Load = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT_VIRAL_Load").Count();
                IMS_TB_Screening = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TB_Screening").Count();
                IMS_TB_Presumptive = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TB_Presumptive").Count();
                IMS_TB_Bacteriology_Diagnosis = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TB_Bacteriology_Diagnosis").Count();
                IMS_TB_Diagnosed = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TB_Diagnosed").Count();
                IMS_TB_Treatment = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TB_Treatment").Count();
                IMS_TPT_Eligible = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TPT_Eligible").Count();
                IMS_TB_ART = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "TB_ART").Count();


                if (PMTCT != 0)
                {
                    state_gsm.Add(new
                    {
                        y = Math.Round(100 * 1.0 * PMTCT / 20, 0),
                        name = state.Key,
                        drilldown = state.Key
                    });
                }
                if (IMS_PMTCT != 0)
                {
                    state_ims.Add(new
                    {
                        y = Math.Round(100 * 1.0 * IMS_PMTCT / 162, 0),
                        name = state.Key,
                        drilldown = state.Key
                    });
                }




                var lga_gsm = new List<dynamic>();
                var lga_ims = new List<dynamic>();

                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    PMTCT = 0;
                    IMS_PMTCT = 0;

                    PMTCT = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT" && x.LGA == lga.Key).Count();

                    IMS_PMTCT = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT" && x.LGA == lga.Key).Count();
                    if (PMTCT != 0)
                    {
                        lga_gsm.Add(new
                        {
                            y = Math.Round(100 * 1.0 * PMTCT / 20, 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }

                    if (IMS_PMTCT != 0)
                    {
                        lga_ims.Add(new
                        {
                            y = Math.Round(100 * 1.0 * IMS_PMTCT / 162, 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    var facility_gsm = new List<dynamic>();
                    var facility_ims = new List<dynamic>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {

                        PMTCT = 0;
                        IMS_PMTCT = 0;

                        PMTCT = lst.Where(x => x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT" && x.LGA == lga.Key && x.Facility == fty.Key).Count();

                        IMS_PMTCT = lst.Where(x => !x.GSM_2 && x.State == state.Key && x.indicator == "PMTCT" && x.LGA == lga.Key && x.Facility == fty.Key).Count();
                        if (PMTCT != 0)
                        {
                            facility_gsm.Add(new
                            {
                                fty.Key,
                                percent = Math.Round(100 * 1.0 * PMTCT / 20, 0),

                            });
                        }
                        if (IMS_PMTCT != 0)
                        {
                            facility_ims.Add(new
                            {
                                fty.Key,
                                percent = Math.Round(100 * 1.0 * IMS_PMTCT / 162, 0),

                            });
                        }
                    }

                    //add facility
                    lga_drill_down.Add(new { name = "GSM", id = lga.Key, data = facility_gsm });
                    lga_drill_down.Add(new { name = "IMS", id = lga.Key, data = facility_ims });

                }
                //add lga
                lga_drill_down.Add(new { name = "GSM", id = state.Key, data = lga_gsm });
                lga_drill_down.Add(new { name = "IMS", id = state.Key, data = lga_ims });

            }

            List<dynamic> Comp_Stat = new List<dynamic>
            {
                new
                {
                    name = "GSM",
                    data = state_gsm
                },
                  new
                {
                    name = "IMS",
                    data = state_ims
                }

            };

            return new
            {
                Comp_Stat,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESSRATE_HTS_Index(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<HTS_Index_Completeness_Rate> lst = Utilities.ConvertToList<HTS_Index_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name); 
            var lga_drill_down = new List<dynamic>();

            var state_genealogy_testing_male = new List<dynamic>();
            var state_genealogy_testing_female = new List<dynamic>();
            var state_partner_testing_male = new List<dynamic>();
            var state_partner_testing_female = new List<dynamic>();

            foreach (var state in groupedData)
            {
               

                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0) { 

                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.GSM_2 && x.Sex == "M").Count() != 0)
                    {
                        state_genealogy_testing_male.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.GSM_2 && x.Sex == "M").Count() / (4 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count()), 0),
                            name = state.Key,
                            drilldown = state.Key+"M"
                        });

                    }
                    else
                    {
                        state_genealogy_testing_male.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key+"M"
                        });
                    }

                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.GSM_2 && x.Sex == "F").Count() != 0)
                    {
                        state_genealogy_testing_female.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.GSM_2 && x.Sex == "F").Count() / (4 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count()), 0),
                            name = state.Key,
                            drilldown = state.Key+"F"
                        });

                    }
                    else
                    {
                        state_genealogy_testing_female.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key+"F"
                        });
                    }


                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.GSM_2 && x.Sex == "M").Count() != 0)
                    {
                        state_partner_testing_male.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.GSM_2 && x.Sex == "M").Count() / (16 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count()), 0),
                            name = state.Key,
                            drilldown = state.Key + "M"
                        });

                    }
                    else
                    {
                        state_partner_testing_male.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + "M"
                        });
                    }

                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.GSM_2 && x.Sex == "F").Count() != 0)
                    {
                        state_partner_testing_female.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.GSM_2 && x.Sex == "F").Count() / (16 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count()), 0),
                            name = state.Key,
                            drilldown = state.Key + "F"
                        });

                    }
                    else
                    {
                        state_partner_testing_female.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + "F"
                        });
                    }




                    var lga_genealogy_testing_male = new List<dynamic>();
                    var lga_genealogy_testing_female = new List<dynamic>();
                    var lga_partner_testing_male = new List<dynamic>();
                    var lga_partner_testing_female = new List<dynamic>();
                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {


                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "M").Count() != 0)
                            {
                                lga_genealogy_testing_male.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "M").Count() / (4 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count()), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + "M"
                                });
                            }
                            else
                            {
                                lga_genealogy_testing_male.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + "M"
                                });
                            }

                            if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "F").Count() != 0)
                            {
                                lga_genealogy_testing_female.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "F").Count() / (4 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count()), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + "F"
                                });
                            }
                            else
                            {
                                lga_genealogy_testing_female.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + "F"
                                });
                            }





                            if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "M").Count() != 0)
                            {
                                lga_partner_testing_male.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "M").Count() / (16 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count()), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + "M"
                                });
                            }
                            else
                            {
                                lga_partner_testing_male.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + "M"
                                });
                            }

                            if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "F").Count() != 0)
                            {
                                lga_partner_testing_female.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Sex == "F").Count() / (16 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count()), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + "F"
                                });
                            }
                            else
                            {
                                lga_partner_testing_female.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + "F"
                                });
                            }


                            var facility_genealogy_testing_male = new List<dynamic>();
                            var facility_genealogy_testing_female = new List<dynamic>();
                            var facility_partner_testing_male = new List<dynamic>();
                            var facility_partner_testing_female = new List<dynamic>();
                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {

                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "M").Count() != 0)
                                    {

                                        facility_genealogy_testing_male.Add(new
                                        {

                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "M").Count() / (4 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count()), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_genealogy_testing_male.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                             

                                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "F").Count() != 0)
                                    {

                                        facility_genealogy_testing_female.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Genealogy Testing Index" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "F").Count() / (4 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count()), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_genealogy_testing_female.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }




                                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "M").Count() != 0)
                                    {

                                        facility_partner_testing_male.Add(new
                                        {

                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "M").Count() / (16 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count()), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_partner_testing_male.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }


                                    if (lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "F").Count() != 0)
                                    {

                                        facility_partner_testing_female.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.TestingType == "Partner Testing" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key && x.Sex == "F").Count() / (16 * siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count()), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_partner_testing_female.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                                }
                            }
                            lga_drill_down.Add(new { name = "Genealogy Testing Index", id = lga.Key + "M", data = facility_genealogy_testing_male });
                            lga_drill_down.Add(new { name = "Genealogy Testing Index", id = lga.Key + "F", data = facility_genealogy_testing_female });


                            lga_drill_down.Add(new { name = "Partner Testing", id = lga.Key + "M", data = facility_partner_testing_male });
                            lga_drill_down.Add(new { name = "Partner Testing", id = lga.Key + "F", data = facility_partner_testing_female });
                        }

                        lga_drill_down.Add(new { name = "Genealogy Testing Index", id = state.Key + "M", data = lga_genealogy_testing_male });
                        lga_drill_down.Add(new { name = "Genealogy Testing Index", id = state.Key + "F", data = lga_genealogy_testing_female });

                        lga_drill_down.Add(new { name = "Partner Testing", id = state.Key + "M", data = lga_partner_testing_male });
                        lga_drill_down.Add(new { name = "Partner Testing", id = state.Key + "F", data = lga_partner_testing_female });
                    }
                }

              
            }

            //var genealogy_male = new
            //{
            //    name = "Genealogy Testing Index",
            //    data = state_genealogy_testing_male,
            //    stack = "Male"
            //};

            //var genealogy_female = new
            //{
            //    name = "Genealogy Testing Index",
            //    data = state_genealogy_testing_female,
            //    stack = "Female"
            //};

            //var partner_male = new
            //{
            //    name = "Partner Testing",
            //    data = state_partner_testing_male,
            //    stack = "Male"
            //};

            //var partner_female = new
            //{
            //    name = "Partner Testing",
            //    data = state_genealogy_testing_female,
            //    stack = "Female"
            //};



            //var male_stack = new List<dynamic>
            //{
            //    genealogy_male,
            //    partner_male,
            //};

            //var female_stack = new List<dynamic>
            //{
            //    genealogy_female,
            //    partner_female,
            //};

            //var partner_testing = new List<dynamic>
            //{
            //    partner_male,
            //    partner_female,
            //};


            List<dynamic> Comp_Stat = new List<dynamic>
            {
                new
                {

                name = "Genealogy Testing Male",
                data = state_genealogy_testing_male,
                stack = "genealogy",

                },
                new
                {

                name = "Genealogy Testing Female",
                data = state_genealogy_testing_female,
                stack = "genealogy"
                },
                new
                {
                name = "Partner Testing Male",
                data = state_partner_testing_male,
                stack = "partner"
                },
                new
                {
                   name = "Partner Testing Female",
                data = state_partner_testing_female,
                stack = "partner"
                },



            };

            return new
            {
                Comp_Stat,
                lga_drill_down,
            };
        }


        public dynamic GenerateCOMPLETENESSRATE_TB_Screened(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<TB_Screened_Completeness_Rate> lst = Utilities.ConvertToList<TB_Screened_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_data = new List<dynamic>();
        

            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.state_name == state.Key && x.Number !="" && x.GSM_2).Count() != 0)
                    {
                        state_data.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.Number != "" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_data.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                
                    var lga_data = new List<dynamic>();
                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {


                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.state_name == state.Key && x.Number != "" && x.lga_name == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_data.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.Number != "" && x.lga_name == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_data.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }

                          
                            var facility_data = new List<dynamic>();
                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {

                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.state_name == state.Key && x.Number != "" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() != 0)
                                    {

                                        facility_data.Add(new
                                        {

                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.Number != "" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_data.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                 
                                }
                            }


                            lga_drill_down.Add(new { name = "Number", id = lga.Key, data = facility_data });
                        }
                    }

                    lga_drill_down.Add(new { name = "Number", id = state.Key, data = lga_data});
                }

            }

            List<dynamic> Comp_Stat = new List<dynamic>
            {
                new
                {
                    name = "Number",
                    data = state_data
                }

            };

            return new
            {
                Comp_Stat,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESSRATE_HTS_Other_PITC(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<HTS_Other_PITC_Completeness_Rate> lst = Utilities.ConvertToList<HTS_Other_PITC_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_blood_bank = new List<dynamic>();
            var state_eye_clinic = new List<dynamic>();
            var state_familiy_planning = new List<dynamic>();
            var state_ent_clinic = new List<dynamic>();

            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.State == state.Key && x.SDP == "Blood Bank" && x.GSM_2).Count() != 0)
                    {
                        state_blood_bank.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.SDP == "Blood Bank" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_blood_bank.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                    if (lst.Where(x => x.State == state.Key && x.SDP == "Eye clinic" && x.GSM_2).Count() != 0)
                    {
                        state_eye_clinic.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.SDP == "Eye clinic" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }
                    else
                    {
                        state_eye_clinic.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }



                    var lga_blood_bank = new List<dynamic>();
                    var lga_eye_clinic = new List<dynamic>();
                    var lga_familiy_planning = new List<dynamic>();
                    var lga_ent_clinic = new List<dynamic>();
                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {

                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.State == state.Key && x.SDP == "Blood Bank" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_blood_bank.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.SDP == "Blood Bank" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_eye_clinic.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }


                            if (lst.Where(x => x.State == state.Key && x.SDP == "Eye clinic" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_eye_clinic.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.SDP == "Eye clinic" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }
                            else
                            {
                                lga_eye_clinic.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }


                            var facility_blood_bank = new List<dynamic>();
                            var facility_eye_clinic = new List<dynamic>();
                            var facility_familiy_planning = new List<dynamic>();
                            var facility_ent_clinic = new List<dynamic>();
                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {
                               
                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.State == state.Key && x.SDP == "Blood Bank" && x.LGA == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() != 0)
                                    {

                                        facility_blood_bank.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.SDP == "Blood Bank" && x.LGA == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_blood_bank.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                    if (lst.Where(x => x.State == state.Key && x.SDP == "Eye clinic" && x.LGA.Trim() == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() != 0)
                                    {
                                        facility_eye_clinic.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.SDP == "Eye clinic" && x.LGA == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_eye_clinic.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                                }
                            }


                            lga_drill_down.Add(new { name = "Blood Bank", id = lga.Key, data = facility_blood_bank });
                            lga_drill_down.Add(new { name = "Eye clinic", id = lga.Key + " ", data = facility_eye_clinic });
                        }
                    }

                    lga_drill_down.Add(new { name = "Blood Bank", id = state.Key, data = lga_blood_bank });
                    lga_drill_down.Add(new { name = "Eye clinic", id = state.Key + " ", data = lga_eye_clinic });
                }

            }

            List<dynamic> Comp_Stat_HTS_Other_PITC = new List<dynamic>
            {
                new
                {
                    name = "Blood Bank",
                    data = state_blood_bank
                },

                 new
                {
                    name = "Eye clinic",
                    data = state_eye_clinic
                }

            };

            return new
            {
                Comp_Stat_HTS_Other_PITC,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESSRATE_ART(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<ART_Completeness_Rate> lst = Utilities.ConvertToList<ART_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_Tx_RET = new List<dynamic>();
            var state_Tx_VLA = new List<dynamic>();
          

            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_RET" && x.GSM_2).Count() != 0)
                    {
                        state_Tx_RET.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_RET" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_Tx_RET.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                    if (lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_VLA" && x.GSM_2).Count() != 0)
                    {
                        state_Tx_VLA.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_VLA" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }
                    else
                    {
                        state_Tx_VLA.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }



                    var lga_Tx_RET = new List<dynamic>();
                    var lga_Tx_VLA = new List<dynamic>();
                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {

                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_RET" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_Tx_RET.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_RET" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_Tx_RET.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }


                            if (lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_VLA" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_Tx_VLA.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_VLA" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }
                            else
                            {
                                lga_Tx_VLA.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }


                            var facility_Tx_RET = new List<dynamic>();
                            var facility_Tx_VLA = new List<dynamic>();
                           
                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {

                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_RET" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {

                                        facility_Tx_RET.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_RET" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_Tx_RET.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                    if (lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_VLA" && x.LGA.Trim() == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {
                                        facility_Tx_VLA.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.IndicatorType == "Tx_VLA" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_Tx_VLA.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                                }
                            }


                            lga_drill_down.Add(new { name = "Tx_RET", id = lga.Key, data = facility_Tx_RET});
                            lga_drill_down.Add(new { name = "Tx_VLA", id = lga.Key + " ", data = facility_Tx_VLA });
                        }
                    }

                    lga_drill_down.Add(new { name = "Tx_RET", id = state.Key, data = lga_Tx_RET });
                    lga_drill_down.Add(new { name = "Tx_VLA", id = state.Key + " ", data = lga_Tx_VLA });
                }

            }

            List<dynamic> Comp_Stat_ART = new List<dynamic>
            {
                new
                {
                    name = "Tx_RET",
                    data = state_Tx_RET
                },

                 new
                {
                    name = "Tx_VLA",
                    data = state_Tx_VLA
                }

            };

            return new
            {
                Comp_Stat_ART,
                lga_drill_down,
            };
        }


        public dynamic GenerateCOMPLETENESSRATE_PMTCT(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<PMTCT_Completeness_Rate> lst = Utilities.ConvertToList<PMTCT_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_new_client = new List<dynamic>();
            var state_known_status = new List<dynamic>();
            var state_known_hiv_pos = new List<dynamic>();
            var state_new_hiv_pos = new List<dynamic>();
            var state_already_on_art = new List<dynamic>();
            var state_new_on_art = new List<dynamic>();

            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.State == state.Key && x.NewClient !="" && x.GSM_2).Count() != 0)
                    {
                        state_new_client.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.NewClient != "" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_new_client.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                    if (lst.Where(x => x.State == state.Key && x.KnownStatus !="" && x.GSM_2).Count() != 0)
                    {
                        state_known_status.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.KnownStatus != "" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }
                    else
                    {
                        state_known_status.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }


                    var lga_new_client = new List<dynamic>();
                    var lga_known_status = new List<dynamic>();
                    var lga_known_hiv_pos = new List<dynamic>();
                    var lga_new_hiv_pos = new List<dynamic>();
                    var lga_already_on_art = new List<dynamic>();
                    var lga_new_on_art = new List<dynamic>();

                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {

                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.State == state.Key && x.NewClient != "" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_new_client.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.NewClient !="" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_new_client.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }


                            if (lst.Where(x => x.State == state.Key && x.KnownStatus !="" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_known_status.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.KnownStatus != "" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }
                            else
                            {
                                lga_known_status.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }


                            var facility_new_client = new List<dynamic>();
                            var facility_known_status = new List<dynamic>();
                            var facility_known_hiv_pos = new List<dynamic>();
                            var facility_new_hiv_pos = new List<dynamic>();
                            var facility_already_on_art = new List<dynamic>();
                            var facility_new_on_art = new List<dynamic>();

                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {

                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.State == state.Key && x.NewClient !="" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {

                                        facility_new_client.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.NewClient != "" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_new_client.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                    if (lst.Where(x => x.State == state.Key && x.KnownStatus != "" && x.LGA.Trim() == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {
                                        facility_known_status.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.KnownStatus != "" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_known_status.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                                }
                            }


                            lga_drill_down.Add(new { name = "New Client", id = lga.Key, data = facility_new_client });
                            lga_drill_down.Add(new { name = "Known Status", id = lga.Key + " ", data = facility_known_status });
                        }
                    }

                    lga_drill_down.Add(new { name = "Known Status", id = state.Key, data = lga_new_client });
                    lga_drill_down.Add(new { name = "New Client", id = state.Key + " ", data = lga_known_status });
                }

            }

            List<dynamic> Comp_Stat_PMTCT = new List<dynamic>
            {
                new
                {
                    name = "Known Status",
                    data = state_new_client
                },

                 new
                {
                    name = "New Client",
                    data = state_known_status
                }

            };

            return new
            {
                Comp_Stat_PMTCT,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESSRATE_PMTCT_EID(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<PMTCT_EID_Completeness_Rate> lst = Utilities.ConvertToList<PMTCT_EID_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_eid_sample_collected = new List<dynamic>();
            var state_eid_pos = new List<dynamic>();
            var state_eid_art_initiation = new List<dynamic>();
           

            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.State == state.Key && x.EID_Sample_Collected != "" && x.GSM_2).Count() != 0)
                    {
                        state_eid_sample_collected.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_Sample_Collected != "" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_eid_sample_collected.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                    if (lst.Where(x => x.State == state.Key && x.EID_POS != "" && x.GSM_2).Count() != 0)
                    {
                        state_eid_pos.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_POS != "" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }
                    else
                    {
                        state_eid_pos.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }

                    if (lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.GSM_2).Count() != 0)
                    {
                        state_eid_art_initiation.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key + "  "
                        });
                    }
                    else
                    {
                        state_eid_art_initiation.Add(new
                        {
                            y = 0.0,
                            name = state.Key, 
                            drilldown = state.Key + "  "
                        });
                    }


                    var lga_eid_sample_collected = new List<dynamic>();
                    var lga_eid_pos = new List<dynamic>();
                    var lga_eid_art_initiation = new List<dynamic>();

                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {

                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_eid_sample_collected.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_eid_sample_collected.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }


                            if (lst.Where(x => x.State == state.Key && x.EID_POS != "" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_eid_pos.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_POS != "" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }
                            else
                            {
                                lga_eid_pos.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }

                            if (lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_eid_art_initiation.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + "  "
                                });
                            }
                            else
                            {
                                lga_eid_art_initiation.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + "  "
                                });
                            }


                            var facility_eid_sample_collected = new List<dynamic>();
                            var facility_eid_pos = new List<dynamic>();
                            var facility_eid_art_initiation = new List<dynamic>();


                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {

                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.State == state.Key && x.EID_Sample_Collected != "" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {

                                        facility_eid_sample_collected.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_Sample_Collected != "" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_eid_sample_collected.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                    if (lst.Where(x => x.State == state.Key && x.EID_POS != "" && x.LGA.Trim() == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {
                                        facility_eid_pos.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_POS != "" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_eid_pos.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }


                                    if (lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.LGA.Trim() == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {
                                        facility_eid_art_initiation.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.EID_ART_Initiation != "" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_eid_art_initiation.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                                }
                            }


                            lga_drill_down.Add(new { name = "EID Sample Collected", id = lga.Key, data = facility_eid_sample_collected });
                            lga_drill_down.Add(new { name = "EID POS", id = lga.Key + " ", data = facility_eid_pos });
                            lga_drill_down.Add(new { name = "EID ART Initiation", id = lga.Key + "  ", data = facility_eid_art_initiation });
                        }
                    }

                    lga_drill_down.Add(new { name = "EID Sample Collected", id = state.Key, data = lga_eid_sample_collected });
                    lga_drill_down.Add(new { name = "EID POS", id = state.Key + " ", data = lga_eid_pos });
                    lga_drill_down.Add(new { name = "EID ART Initiation", id = state.Key + "  ", data = lga_eid_art_initiation });
                }

            }

            List<dynamic> Comp_Stat_PMTCT_EID = new List<dynamic>
            {
                new
                {
                    name = "EID Sample Collected",
                    data = state_eid_sample_collected
                },

                 new
                {
                    name = "EID POS",
                    data = state_eid_pos
                },

                 new
                {
                    name = "EID ART Initiation",
                    data = state_eid_art_initiation
                }

            };

            return new
            {
                Comp_Stat_PMTCT_EID,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESSRATE_HTS_TST(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<HTS_TST_Completeness_Rate> lst = Utilities.ConvertToList<HTS_TST_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_emergency = new List<dynamic>();
            var state_in_patient = new List<dynamic>();
            var state_malnutrition_clinic = new List<dynamic>();
            var state_under_5_paediatrics = new List<dynamic>();
            var state_STI_clinic = new List<dynamic>();
            var state_TB = new List<dynamic>();
            var state_VCT = new List<dynamic>();
            var state_PMTCT_post_ANC1 = new List<dynamic>();
            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.state_name == state.Key && x.SDP == "Emergency" && x.GSM_2).Count() != 0)
                    {
                        state_emergency.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.SDP == "Emergency" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_emergency.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                



                    var lga_emergency = new List<dynamic>();
                    var lga_in_patient = new List<dynamic>();
                    var lga_malnutrition_clinic = new List<dynamic>();
                    var lga_under_5_paediatrics = new List<dynamic>();
                    var lga_STI_clinic = new List<dynamic>();
                    var lga_TB = new List<dynamic>();
                    var lga_VCT = new List<dynamic>();
                    var lga_PMTCT_post_ANC1 = new List<dynamic>();
                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {

                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.state_name == state.Key && x.SDP == "Emergency" && x.lga_name == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_emergency.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.SDP == "Emergency" && x.lga_name == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_emergency.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }


                         

                            var facility_emergency = new List<dynamic>();
                            var facility_in_patient = new List<dynamic>();
                            var facility_malnutrition_clinic = new List<dynamic>();
                            var facility_under_5_paediatrics = new List<dynamic>();
                            var facility_STI_clinic = new List<dynamic>();
                            var facility_TB = new List<dynamic>();
                            var facility_VCT = new List<dynamic>();
                            var facility_PMTCT_post_ANC1 = new List<dynamic>();
                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {
                                string test = fty.Key;
                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.state_name == state.Key && x.SDP == "Emergency" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() != 0)
                                    {

                                        facility_emergency.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.state_name == state.Key && x.SDP == "Emergency" && x.lga_name == lga.Key && x.GSM_2 && x.Name == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_emergency.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                    
                                }
                            }


                            lga_drill_down.Add(new { name = "Emergency", id = lga.Key, data = facility_emergency });
                            lga_drill_down.Add(new { name = "In-Patient", id = lga.Key+" ", data = facility_in_patient });
                            lga_drill_down.Add(new { name = "Malnutrition Clinic", id = lga.Key + "  ", data = facility_malnutrition_clinic });
                            lga_drill_down.Add(new { name = "Under 5 Paediatrics", id = lga.Key + "   ", data = facility_under_5_paediatrics });
                            lga_drill_down.Add(new { name = "STI", id = lga.Key + "    ", data = facility_STI_clinic });
                            lga_drill_down.Add(new { name = "TB Clinic", id = lga.Key + "      ", data = facility_TB });
                            lga_drill_down.Add(new { name = "VCT", id = lga.Key + "       ", data = facility_VCT });
                            lga_drill_down.Add(new { name = "PMTCT (Post ANC1)", id = lga.Key + "         ", data = facility_PMTCT_post_ANC1 });
                        }
                    }

                    lga_drill_down.Add(new { name = "Emergency", id = state.Key, data = lga_emergency });
                    lga_drill_down.Add(new { name = "In-Patient", id = state.Key + " ", data = lga_in_patient });
                    lga_drill_down.Add(new { name = "Malnutrition Clinic", id = state.Key + "  ", data = lga_malnutrition_clinic });
                    lga_drill_down.Add(new { name = "Under 5 Paediatrics", id = state.Key + "   ", data = lga_under_5_paediatrics });
                    lga_drill_down.Add(new { name = "STI", id = state.Key + "     ", data = lga_STI_clinic });
                    lga_drill_down.Add(new { name = "TB Clinic", id = state.Key + "       ", data = lga_TB });
                    lga_drill_down.Add(new { name = "VCT", id = state.Key + "        ", data = lga_VCT });
                    lga_drill_down.Add(new { name = "PMTCT (Post ANC1)", id = state.Key + "          ", data = lga_PMTCT_post_ANC1 });
                }

            }

            List<dynamic> Comp_Stat_HTS_TST = new List<dynamic>
            {
               
            new
                {
                    name = "Emergency",
                    data = state_emergency
                },
                new
                {
                    name = "In-Patient",
                    data = state_in_patient
                },
                new
                {
                    name = "Malnutrition Clinic",
                    data = state_malnutrition_clinic
                },
                new
                {
                    name = "Under 5 Paediatrics",
                    data = state_under_5_paediatrics
                },
                new
                {
                    name = "STI",
                    data = state_STI_clinic
                },
                new
                {
                    name = "TB Clinic",
                    data = state_TB
                },
                new
                {
                    name = "VCT",
                    data = state_VCT
                },
                new
                {
                    name = "PMTCT (Post ANC1)",
                    data = state_PMTCT_post_ANC1
                }

            };

            return new
            {
                Comp_Stat_HTS_TST,
                lga_drill_down,
            };
        }

        public dynamic GenerateCOMPLETENESSRATE_PMTCT_Viral_Load(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<PMTCT_Viral_Load_Completeness_Rate> lst = Utilities.ConvertToList<PMTCT_Viral_Load_Completeness_Rate>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_newly_identified = new List<dynamic>();
            var state_already_hiv_pos = new List<dynamic>();
          

            foreach (var state in groupedData)
            {


                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2).Count() > 0)
                {

                    if (lst.Where(x => x.State == state.Key && x.Category == "Newly_Identified" && x.GSM_2).Count() != 0)
                    {
                        state_newly_identified.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.Category == "Newly_Identified" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key
                        });

                    }
                    else
                    {
                        state_newly_identified.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key
                        });
                    }

                    if (lst.Where(x => x.State == state.Key && x.Category == "Already_HIV_Positive" && x.GSM_2).Count() != 0)
                    {
                        state_already_hiv_pos.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.Category == "Already_HIV_Positive" && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }
                    else
                    {
                        state_already_hiv_pos.Add(new
                        {
                            y = 0.0,
                            name = state.Key,
                            drilldown = state.Key + " "
                        });
                    }



                    var lga_newly_identified = new List<dynamic>();
                    var lga_already_hiv_pos = new List<dynamic>();



                    foreach (var lga in state.GroupBy(x => x.lga_name))
                    {

                        if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key).Count() > 0)
                        {

                            if (lst.Where(x => x.State == state.Key && x.Category == "Newly_Identified" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_newly_identified.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.Category == "Newly_Identified" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }
                            else
                            {
                                lga_newly_identified.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key
                                });
                            }


                            if (lst.Where(x => x.State == state.Key && x.Category == "Already_HIV_Positive" && x.LGA == lga.Key && x.GSM_2).Count() != 0)
                            {
                                lga_already_hiv_pos.Add(new
                                {
                                    y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.Category == "Already_HIV_Positive" && x.LGA == lga.Key && x.GSM_2).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }
                            else
                            {
                                lga_already_hiv_pos.Add(new
                                {
                                    y = 0.0,
                                    name = lga.Key,
                                    drilldown = lga.Key + " "
                                });
                            }


                            var facility_newly_identified = new List<dynamic>();
                            var facility_already_hiv_pos = new List<dynamic>();

                            foreach (var fty in lga.GroupBy(x => x.Name))
                            {

                                if (siteLst.Where(x => x.state_name == state.Key && x.GSM_2 && x.lga_name == lga.Key && x.Name == fty.Key).Count() > 0)
                                {

                                    if (lst.Where(x => x.State == state.Key && x.Category == "Newly_Identified" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {

                                        facility_newly_identified.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.Category == "Newly_Identified" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_newly_identified.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }

                                    if (lst.Where(x => x.State == state.Key && x.Category == "Already_HIV_Positive" && x.LGA.Trim() == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() != 0)
                                    {
                                        facility_already_hiv_pos.Add(new
                                        {
                                            name = fty.Key,
                                            y = Math.Round(100 * 1.0 * lst.Where(x => x.State == state.Key && x.Category == "Already_HIV_Positive" && x.LGA == lga.Key && x.GSM_2 && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                                        });
                                    }
                                    else
                                    {
                                        facility_already_hiv_pos.Add(new
                                        {
                                            name = fty.Key,
                                            y = 0.0,
                                        });
                                    }
                                }
                            }


                            lga_drill_down.Add(new { name = "Newly Identified", id = lga.Key, data = facility_newly_identified });
                            lga_drill_down.Add(new { name = "Already HIV Positive", id = lga.Key + " ", data = facility_already_hiv_pos });
                        }
                    }

                    lga_drill_down.Add(new { name = "Newly Identified", id = state.Key, data = lga_newly_identified });
                    lga_drill_down.Add(new { name = "Already HIV Positive", id = state.Key + " ", data = lga_already_hiv_pos });
                }

            }

            List<dynamic> Comp_Stat_PMTCT_Viral_Load = new List<dynamic>
            {
                new
                {
                    name = "Newly Identified",
                    data = state_newly_identified
                },

                 new
                {
                    name = "Already HIV Positive",
                    data = state_already_hiv_pos
                }

            };

            return new
            {
                Comp_Stat_PMTCT_Viral_Load,
                lga_drill_down,
            };
        }
        public dynamic GenerateREPORTINGRATE(DataTable dt)
        {
            MPMDAO dao = new MPMDAO();

            List<GranularSites> siteLst = Utilities.ConvertToList<GranularSites>(dao.exeCuteStoredProcedure("sp_mpm_granular_sites"));
            List<CompletenessReport> lst = Utilities.ConvertToList<CompletenessReport>(dt);
            var groupedData = siteLst.GroupBy(x => x.state_name);
            var lga_drill_down = new List<dynamic>();

            var state_gsm = new List<dynamic>();
            foreach (var state in groupedData)
            {

                if (lst.Where(x => x.GSM_2 && (x.indicator == "PMTCT" || x.indicator == "PMTCT_EID" || x.indicator == "ART" || x.indicator == "HTS" || x.indicator == "HTS_PITC" || x.indicator == "Linkage_To_Treatment" || x.indicator == "PMTCT_VIRAL_Load" || x.indicator == "TB_Screening" || x.indicator == "TB_Presumptive" || x.indicator == "TB_Bacteriology_Diagnosis" || x.indicator == "TB_Diagnosed" || x.indicator == "TB_Treatment" || x.indicator == "TPT_Eligible" || x.indicator == "TB_ART" ) && x.State == state.Key).Count() != 0)
                {
                    state_gsm.Add(new
                    {
                        y = Math.Round(100 * 1.0 * lst.Where(x => x.GSM_2 && (x.indicator == "PMTCT" || x.indicator == "PMTCT_EID" || x.indicator == "ART" || x.indicator == "HTS" || x.indicator == "HTS_PITC" || x.indicator == "Linkage_To_Treatment" || x.indicator == "PMTCT_VIRAL_Load" || x.indicator == "TB_Screening" || x.indicator == "TB_Presumptive" || x.indicator == "TB_Bacteriology_Diagnosis" || x.indicator == "TB_Diagnosed" || x.indicator == "TB_Treatment" || x.indicator == "TPT_Eligible" || x.indicator == "TB_ART") && x.State == state.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key).Count(), 0),
                        name = state.Key,
                        drilldown = state.Key
                    });
                }
                else
                {

                    state_gsm.Add(new
                    {
                        y = 0.0,
                        name = state.Key,
                        drilldown = state.Key
                    });
                }

                var lga_gsm = new List<dynamic>();
                foreach (var lga in state.GroupBy(x => x.lga_name))
                {
                    if (lst.Where(x => x.GSM_2 && (x.indicator == "PMTCT" || x.indicator == "PMTCT_EID" || x.indicator == "ART" || x.indicator == "HTS" || x.indicator == "HTS_PITC" || x.indicator == "Linkage_To_Treatment" || x.indicator == "PMTCT_VIRAL_Load" || x.indicator == "TB_Screening" || x.indicator == "TB_Presumptive" || x.indicator == "TB_Bacteriology_Diagnosis" || x.indicator == "TB_Diagnosed" || x.indicator == "TB_Treatment" || x.indicator == "TPT_Eligible" || x.indicator == "TB_ART") && x.State == state.Key && x.LGA == lga.Key).Count() != 0)
                    {
                        lga_gsm.Add(new
                        {
                            y = Math.Round(100 * 1.0 * lst.Where(x => x.GSM_2 && (x.indicator == "PMTCT" || x.indicator == "PMTCT_EID" || x.indicator == "ART" || x.indicator == "HTS" || x.indicator == "HTS_PITC" || x.indicator == "Linkage_To_Treatment" || x.indicator == "PMTCT_VIRAL_Load" || x.indicator == "TB_Screening" || x.indicator == "TB_Presumptive" || x.indicator == "TB_Bacteriology_Diagnosis" || x.indicator == "TB_Diagnosed" || x.indicator == "TB_Treatment" || x.indicator == "TPT_Eligible" || x.indicator == "TB_ART") && x.State == state.Key && x.LGA == lga.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key).Count(), 0),
                            name = lga.Key,
                            drilldown = lga.Key
                        });
                    }
                    else
                    {
                        lga_gsm.Add(new
                        {
                            y = 0.0,
                            name = lga.Key,
                            drilldown = lga.Key
                        });
                    }


                    var facility_gsm = new List<dynamic>();
                    foreach (var fty in lga.GroupBy(x => x.Name))
                    {
                        if (lst.Where(x => x.GSM_2 && (x.indicator == "PMTCT" || x.indicator == "PMTCT_EID" || x.indicator == "ART" || x.indicator == "HTS" || x.indicator == "HTS_PITC" || x.indicator == "Linkage_To_Treatment" || x.indicator == "PMTCT_VIRAL_Load" || x.indicator == "TB_Screening" || x.indicator == "TB_Presumptive" || x.indicator == "TB_Bacteriology_Diagnosis" || x.indicator == "TB_Diagnosed" || x.indicator == "TB_Treatment" || x.indicator == "TPT_Eligible" || x.indicator == "TB_ART") && x.State == state.Key && x.LGA == lga.Key && x.Facility == fty.Key).Count() != 0)
                        {
                            facility_gsm.Add(new
                            {
                             name = fty.Key,
                                y = Math.Round(100 * 1.0 * lst.Where(x => x.GSM_2 && (x.indicator == "PMTCT" || x.indicator == "PMTCT_EID" || x.indicator == "ART" || x.indicator == "HTS" || x.indicator == "HTS_PITC" || x.indicator == "Linkage_To_Treatment" || x.indicator == "PMTCT_VIRAL_Load" || x.indicator == "TB_Screening" || x.indicator == "TB_Presumptive" || x.indicator == "TB_Bacteriology_Diagnosis" || x.indicator == "TB_Diagnosed" || x.indicator == "TB_Treatment" || x.indicator == "TPT_Eligible" || x.indicator == "TB_ART") &&  x.State == state.Key && x.LGA == lga.Key && x.Facility == fty.Key).Count() / siteLst.Where(x => x.GSM_2 && x.state_name == state.Key && x.lga_name == lga.Key && x.Name == fty.Key).Count(), 0),
                            });
                        }
                        else
                        {
                            facility_gsm.Add(new
                            {
                              name =  fty.Key,
                                y = 0.0,
                            });
                        }
                    }
                    lga_drill_down.Add(new { name = "GSM", id = lga.Key, data = facility_gsm });
                }

                lga_drill_down.Add(new { name = "GSM", id = state.Key, data = lga_gsm });

            }

            List<dynamic> Comp_Stat = new List<dynamic>
            {
                new
                {
                    name = "GSM",
                    data = state_gsm
                }

            };

            return new
            {
                Comp_Stat,
                lga_drill_down,
            };
        }

        public dynamic GeneratePMTCT_Cascade(DataTable dt)
        {
            List<PMTCT_Cascade_ViewModel> _dt = Utilities.ConvertToList<PMTCT_Cascade_ViewModel>(dt);

            var groupedData = _dt.OrderBy(x => x.State).GroupBy(x => x.State);
            //var _data = new List<dynamic>();

            var _newClient_State = new List<dynamic>();
            var _knownStatus_State = new List<dynamic>();

            List<dynamic> lga_data_maternal_uptake = new List<dynamic>();

            var hiv_pos_reporting_period_state = new List<dynamic>();
            var hiv_pos_on_ART_state = new List<dynamic>();
            var hiv_exposed_tested_at_2month_state = new List<dynamic>();
            var hiv_exposed_tested_at_12mnth_state = new List<dynamic>();

            var hiv_pos_infant_at_2mnth_state = new List<dynamic>();
            var hiv_pos_infant_at_12mnth_state = new List<dynamic>();
            var eid_art_initiation_state = new List<dynamic>();

            List<dynamic> lga_maternal_clinical_cascade = new List<dynamic>();

            var lga_infant_linkage = new List<dynamic>();

            foreach (var state in groupedData)
            {
                _newClient_State.Add(new
                {
                    name = state.Key,
                    y = state.Sum(x => x.NewClient),
                    drilldown = state.Key
                });
                _knownStatus_State.Add(new
                {
                    name = state.Key,
                    y = state.Sum(x => x.KnownStatus),
                    drilldown = state.Key + " "
                });

                //maternal and clinical 
                hiv_pos_reporting_period_state.Add(new
                {
                    y = state.Sum(x => x.KnownHIVPos) + state.Sum(x => x.NewHIVPos),
                    name = state.Key,
                    drilldown = state.Key
                });
                hiv_pos_on_ART_state.Add(new
                {
                    y = state.Sum(x => x.AlreadyOnART) + state.Sum(x => x.NewOnART),
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                hiv_exposed_tested_at_2month_state.Add(new
                {
                    y = state.Where(x => x.AgeGroup == "0<=2").Sum(x => x.EID_Sample_Collected),
                    name = state.Key,
                    drilldown = state.Key + "  "
                });
                hiv_exposed_tested_at_12mnth_state.Add(new
                {
                    y = state.Where(x => x.AgeGroup == "2-12mths").Sum(x => x.EID_Sample_Collected),
                    name = state.Key,
                    drilldown = state.Key + "   "
                });

                //infant_linkage
                eid_art_initiation_state.Add(new
                {
                    y = state.Sum(x => x.EID_ART_Initiation),
                    name = state.Key,
                    drilldown = state.Key
                });
                hiv_pos_infant_at_12mnth_state.Add(new
                {
                    y = state.Where(x => x.AgeGroup == "2-12mths").Sum(x => x.EID_POS),
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                hiv_pos_infant_at_2mnth_state.Add(new
                {
                    y = state.Where(x => x.AgeGroup == "0<=2").Sum(x => x.EID_POS),
                    name = state.Key,
                    drilldown = state.Key + "  "
                });

                List<dynamic> lga_data_newClient = new List<dynamic>();
                List<dynamic> lga_data_knowStatus = new List<dynamic>();

                var hiv_pos_reporting_period_lga = new List<dynamic>();
                var hiv_pos_on_ART_lga = new List<dynamic>();
                var hiv_exposed_tested_at_2month_lga = new List<dynamic>();
                var hiv_exposed_tested_at_12mnth_lga = new List<dynamic>();

                var lga_hiv_pos_infant_at_2mnth = new List<dynamic>();
                var lga_hiv_pos_infant_at_12mnth = new List<dynamic>();
                var lga_eid_art_initiation = new List<dynamic>();

                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    lga_data_newClient.Add(new
                    {
                        name = lga.Key,
                        y = lga.Sum(x => x.NewClient),
                        drilldown = lga.Key
                    });
                    lga_data_knowStatus.Add(new
                    {
                        name = lga.Key,
                        y = lga.Sum(x => x.KnownStatus),
                        drilldown = lga.Key + " "
                    });

                    //lga_maternal_clinical_cascade
                    hiv_pos_reporting_period_lga.Add(new
                    {
                        y = lga.Sum(x => x.KnownHIVPos) + lga.Sum(x => x.NewHIVPos),
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    hiv_pos_on_ART_lga.Add(new
                    {
                        y = lga.Sum(x => x.AlreadyOnART) + lga.Sum(x => x.NewOnART),
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    hiv_exposed_tested_at_2month_lga.Add(new
                    {
                        y = lga.Where(x => x.AgeGroup == "0<=2").Sum(x => x.EID_Sample_Collected),
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });
                    hiv_exposed_tested_at_12mnth_lga.Add(new
                    {
                        y = lga.Where(x => x.AgeGroup == "2-12mths").Sum(x => x.EID_Sample_Collected),
                        name = lga.Key,
                        drilldown = lga.Key + "   "
                    });

                    //infant_linkage
                    lga_eid_art_initiation.Add(new
                    {
                        y = lga.Sum(x => x.EID_ART_Initiation),
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_hiv_pos_infant_at_12mnth.Add(new
                    {
                        y = lga.Where(x => x.AgeGroup == "2-12mths").Sum(x => x.EID_POS),
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_hiv_pos_infant_at_2mnth.Add(new
                    {
                        y = lga.Where(x => x.AgeGroup == "0<=2").Sum(x => x.EID_POS),
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });

                    List<dynamic> facility_data_newClient = new List<dynamic>();
                    List<dynamic> facility_data_knowStatus = new List<dynamic>();

                    var hiv_pos_reporting_period_facility = new List<dynamic>();
                    var hiv_pos_on_ART_facility = new List<dynamic>();
                    var hiv_exposed_tested_at_2month_facility = new List<dynamic>();
                    var hiv_exposed_tested_at_12mnth_facility = new List<dynamic>();

                    var facility_hiv_pos_infant_at_2mnth = new List<dynamic>();
                    var facility_hiv_pos_infant_at_12mnth = new List<dynamic>();
                    var facility_eid_art_initiation = new List<dynamic>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facility_data_newClient.Add(new List<dynamic>
                        {
                            fty.Key, fty.Sum(x => x.NewClient),
                        });
                        facility_data_knowStatus.Add(new List<dynamic>
                        {
                            fty.Key, fty.Sum(x => x.KnownStatus),
                        });

                        hiv_pos_reporting_period_facility.Add(new List<dynamic>
                        {
                            fty.Key, fty.Sum(x => x.KnownHIVPos) + fty.Sum(x => x.NewHIVPos),
                        });
                        hiv_pos_on_ART_facility.Add(new List<dynamic>
                        {
                            fty.Key, fty.Sum(x => x.AlreadyOnART) + fty.Sum(x => x.NewOnART)
                        });
                        hiv_exposed_tested_at_2month_facility.Add(new List<dynamic>
                        {
                            fty.Key, fty.Where(x => x.AgeGroup == "0<=2").Sum(x => x.EID_Sample_Collected),
                        });
                        hiv_exposed_tested_at_12mnth_facility.Add(new List<dynamic>
                        {
                            fty.Key, fty.Where(x => x.AgeGroup == "2-12mths").Sum(x => x.EID_Sample_Collected),
                        });

                        //infant_linkage 
                        facility_eid_art_initiation.Add(new List<dynamic>
                        {
                            fty.Key, fty.Sum(x => x.EID_ART_Initiation)
                        });
                        facility_hiv_pos_infant_at_2mnth.Add(new List<dynamic>
                        {
                            fty.Key, fty.Where(x => x.AgeGroup == "0<=2").Sum(x => x.EID_POS),
                        });
                        facility_hiv_pos_infant_at_12mnth.Add(new List<dynamic>
                        {
                            fty.Key, fty.Where(x => x.AgeGroup == "2-12mths").Sum(x => x.EID_POS),
                        });
                    }
                    lga_data_maternal_uptake.Add(new { name = "New ANC Client", id = lga.Key, data = facility_data_newClient });
                    lga_data_maternal_uptake.Add(new { name = "Known Status", id = lga.Key + " ", data = facility_data_knowStatus });

                    //lga_maternal_clinical_cascade
                    lga_maternal_clinical_cascade.Add(new { name = "HIV+ pregenant women in the reporting Period", id = lga.Key, data = hiv_pos_reporting_period_facility });
                    lga_maternal_clinical_cascade.Add(new { name = "HIV+ pregenant women receiving ARVs", id = lga.Key + " ", data = hiv_pos_on_ART_facility });
                    lga_maternal_clinical_cascade.Add(new { name = "HIV exposed infants tested by 2 months", id = lga.Key + "  ", data = hiv_exposed_tested_at_2month_facility });
                    lga_maternal_clinical_cascade.Add(new { name = "HIV exposed infants tested by 12 months", id = lga.Key + "   ", data = hiv_exposed_tested_at_12mnth_facility });

                    //infant_linkage
                    lga_infant_linkage.Add(new { name = "HIV Infected Infants New on ART", id = lga.Key, data = facility_eid_art_initiation });
                    lga_infant_linkage.Add(new { name = "HIV infected Infants Identified by 2 Months", id = lga.Key + " ", data = facility_hiv_pos_infant_at_2mnth });
                    lga_infant_linkage.Add(new { name = "HIV Infected Infants Identified by 12 months", id = lga.Key + "  ", data = facility_hiv_pos_infant_at_12mnth });
                }
                lga_data_maternal_uptake.Add(new { name = "New ANC Client", id = state.Key, data = lga_data_newClient });
                lga_data_maternal_uptake.Add(new { name = "Known Status", id = state.Key + " ", data = lga_data_knowStatus });

                //lga_maternal_clinical_cascade
                lga_maternal_clinical_cascade.Add(new { name = "HIV+ pregenant women in the reporting Period", id = state.Key, data = hiv_pos_reporting_period_lga });
                lga_maternal_clinical_cascade.Add(new { name = "HIV+ pregenant women receiving ARVs", id = state.Key + " ", data = hiv_pos_on_ART_lga });
                lga_maternal_clinical_cascade.Add(new { name = "HIV exposed infants tested by 2 months", id = state.Key + "  ", data = hiv_exposed_tested_at_2month_lga });
                lga_maternal_clinical_cascade.Add(new { name = "HIV exposed infants tested by 12 months", id = state.Key + "   ", data = hiv_exposed_tested_at_12mnth_lga });

                //infant_linkage
                lga_infant_linkage.Add(new { name = "HIV Infected Infants New on ART", id = state.Key, data = lga_eid_art_initiation });
                lga_infant_linkage.Add(new { name = "HIV infected Infants Identified by 2 Months", id = state.Key + " ", data = lga_hiv_pos_infant_at_2mnth });
                lga_infant_linkage.Add(new { name = "HIV Infected Infants Identified by 12 months", id = state.Key + "  ", data = lga_hiv_pos_infant_at_12mnth });
            }

            List<dynamic> state_data_maternal_uptake = new List<dynamic>
            {
                new
                {
                    name = "New ANC Client",
                    data = _newClient_State
                },
                new
                {
                    name = "Known Status",
                    data = _knownStatus_State
                }
            };


            List<dynamic> state_maternal_clinical_cascade = new List<dynamic>
            {
                new
                {
                    name = "HIV+ pregenant women in the reporting Period",
                    data = hiv_pos_reporting_period_state
                },
                new
                {
                    name = "HIV+ pregenant women receiving ARVs",
                    data = hiv_pos_on_ART_state
                },
                new
                {
                    name = "HIV exposed infants tested by 2 months",
                    data = hiv_exposed_tested_at_2month_state
                },
                new
                {
                    name = "HIV exposed infants tested by 12 months",
                    data = hiv_exposed_tested_at_12mnth_state
                }
            };

            List<dynamic> state_infant_linkage = new List<dynamic>
            {
                new
                {
                    name = "HIV infected Infants Identified by 2 Months",
                    data = hiv_pos_infant_at_2mnth_state
                },
                new
                {
                    name = "HIV Infected Infants Identified by 12 months",
                    data = hiv_pos_infant_at_12mnth_state
                },
                new
                {
                    name = "HIV Infected Infants New on ART",
                    data = eid_art_initiation_state
                }
            };

            return new
            {
                state_data_maternal_uptake,
                lga_data_maternal_uptake,
                state_maternal_clinical_cascade,
                lga_maternal_clinical_cascade,
                state_infant_linkage,
                lga_infant_linkage,
            };
        }


        public dynamic GeneratePMTCT_VL(DataTable dt)
        {
            List<PMTCT_VL_ViewModel> lst = Utilities.ConvertToList<PMTCT_VL_ViewModel>(dt);

            var groupedData = lst.GroupBy(x => x.State);

            List<dynamic> already_pos_suppressed_state = new List<dynamic>();
            List<dynamic> already_pos_unsuppressed_state = new List<dynamic>();
            List<dynamic> new_pos_suppressed_state = new List<dynamic>();
            List<dynamic> new_pos_unsuppressed_state = new List<dynamic>();

            List<dynamic> lga_drill_down_already_pos_suppressed = new List<dynamic>();
            List<dynamic> lga_drill_down_already_pos_unsuppressed = new List<dynamic>();

            List<dynamic> lga_drilldown_new_pos_suppressed = new List<dynamic>();
            List<dynamic> lga_drilldown_new_pos_unsuppressed = new List<dynamic>();

            var drilldown_series_known_status = new List<dynamic>();
            var drilldown_series_new_pos = new List<dynamic>();

            foreach (var state in groupedData)
            {
                already_pos_suppressed_state.Add(new
                {
                    name = state.Key,
                    y = state.Where(x => x.Category == "Already HIV Positive" && x.ResultGroup == "Suppressed")
                    .Sum(x => x.Result),
                    drilldown = state.Key + "_suppressed"
                });
                already_pos_unsuppressed_state.Add(new
                {
                    name = state.Key,
                    y = state.Where(x => x.Category == "Already HIV Positive" && x.ResultGroup == "Not Suppressed")
                    .Sum(x => x.Result),
                    drilldown = state.Key + "_unsuppressed"
                });

                new_pos_suppressed_state.Add(new
                {
                    name = state.Key,
                    y = state.Where(x => x.Category == "Newly Identified" && x.ResultGroup == "Suppressed")
                    .Sum(x => x.Result),
                    drilldown = state.Key + "_suppressed"
                });
                new_pos_unsuppressed_state.Add(new
                {
                    name = state.Key,
                    y = state.Where(x => x.Category == "Newly Identified" && x.ResultGroup == "Not Suppressed")
                        .Sum(x => x.Result),
                    drilldown = state.Key + "_unsuppressed"
                });

                List<dynamic> lga_data_already_pos_suppressed = new List<dynamic>();
                List<dynamic> lga_data_already_pos_unsuppressed = new List<dynamic>();
                List<dynamic> lga_data_new_pos_supressed = new List<dynamic>();
                List<dynamic> lga_data_new_pos_unsupressed = new List<dynamic>();

                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    lga_data_already_pos_suppressed.Add(new
                    {
                        name = lga.Key,
                        y = lga.Where(x => x.Category == "Already HIV Positive" && x.ResultGroup == "Suppressed")
                    .Sum(x => x.Result),
                        drilldown = lga.Key + "_suppressed"
                    });
                    lga_data_already_pos_unsuppressed.Add(new
                    {
                        name = lga.Key,
                        y = lga.Where(x => x.Category == "Already HIV Positive" && x.ResultGroup == "Not Suppressed")
                    .Sum(x => x.Result),
                        drilldown = lga.Key + "_unsuppressed"
                    });
                    lga_data_new_pos_supressed.Add(new
                    {
                        name = lga.Key,
                        y = lga.Where(x => x.Category == "Newly Identified" && x.ResultGroup == "Suppressed")
                    .Sum(x => x.Result),
                        drilldown = lga.Key + "_suppressed"
                    });
                    lga_data_new_pos_unsupressed.Add(new
                    {
                        name = lga.Key,
                        y = lga.Where(x => x.Category == "Newly Identified" && x.ResultGroup == "Not Suppressed")
                        .Sum(x => x.Result),
                        drilldown = lga.Key + "_unsuppressed"
                    });

                    List<dynamic> facility_data_new_pos_supressed = new List<dynamic>();
                    List<dynamic> facility_data_new_pos_unsupressed = new List<dynamic>();
                    List<dynamic> facility_data_already_pos_suppressed = new List<dynamic>();
                    List<dynamic> facility_data_already_pos_unsuppressed = new List<dynamic>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facility_data_already_pos_suppressed.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Where(x => x.Category == "Already HIV Positive" && x.ResultGroup == "Suppressed")
                            .Sum(x => x.Result),
                        });
                        facility_data_already_pos_unsuppressed.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Where(x => x.Category == "Already HIV Positive" && x.ResultGroup == "Not Suppressed")
                            .Sum(x => x.Result),
                        });
                        facility_data_new_pos_supressed.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Where(x => x.Category == "Newly Identified" && x.ResultGroup == "Suppressed")
                                .Sum(x => x.Result),
                        });
                        facility_data_new_pos_unsupressed.Add(new List<dynamic>
                        {
                            fty.Key,
                            fty.Where(x => x.Category == "Newly Identified" && x.ResultGroup == "Not Suppressed")
                                .Sum(x => x.Result),
                        });
                    }
                    drilldown_series_known_status.Add(new { name = "Suppressed", id = lga.Key + "_suppressed", data = facility_data_already_pos_suppressed });
                    drilldown_series_known_status.Add(new { name = "Not Suppressed", id = lga.Key + "_unsuppressed", data = facility_data_already_pos_unsuppressed });

                    drilldown_series_new_pos.Add(new { name = "Suppressed", id = lga.Key + "_suppressed", data = facility_data_new_pos_supressed });
                    drilldown_series_new_pos.Add(new { name = "Not Suppressed", id = lga.Key + "_unsuppressed", data = facility_data_new_pos_unsupressed });
                }
                drilldown_series_known_status.Add(new { name = "Suppressed", id = state.Key + "_suppressed", data = lga_data_already_pos_suppressed });
                drilldown_series_known_status.Add(new { name = "Not Suppressed", id = state.Key + "_unsuppressed", data = lga_data_already_pos_unsuppressed });

                drilldown_series_new_pos.Add(new { name = "Suppressed", id = state.Key + "_suppressed", data = lga_data_new_pos_supressed });
                drilldown_series_new_pos.Add(new { name = "Not Suppressed", id = state.Key + "_unsuppressed", data = lga_data_new_pos_unsupressed });
            }

            var known_suppressed = new
            {
                name = "Suppressed",
                data = already_pos_suppressed_state,
                stack = "Already HIV Positive"
            };

            var known_unsuppressed = new
            {
                name = "Not Suppressed",
                data = already_pos_unsuppressed_state,
                stack = "Already HIV Positive"
            };

            var newly_suppressed = new
            {
                name = "Suppressed",
                data = new_pos_suppressed_state,
                stack = "Newly Identified"
            };
            var newly_unsuppressed = new
            {
                name = "Not Suppressed",
                data = new_pos_unsuppressed_state,
                stack = "Newly Identified"
            };

            var known_status = new List<dynamic>
            {
                known_unsuppressed,
                known_suppressed,
            };

            var newly_known = new List<dynamic>
            {
                newly_unsuppressed,
                newly_suppressed,
            };

            //drilldown_series_known_status.AddRange(drilldown_series_new_pos),
            return new
            {
                known_status,
                drilldown_series_known_status,
                newly_known,
                drilldown_series_new_pos
            };
        }


        public dynamic GenerateLinkageData(DataTable dt)
        {
            List<LinkageViewModel> lst = Utilities.ConvertToList<LinkageViewModel>(dt);
            var groupedData = lst.GroupBy(x => x.State);

            var pos_state = new List<dynamic>();
            var tx_new_state = new List<dynamic>();
            var linkage_state = new List<dynamic>();

            var lga_drill_down = new List<dynamic>();

            foreach (var state in groupedData)
            {
                var tx_new = state.Where(x => x.Row_Number == 1).Sum(x => x.Tx_NEW);
                var pos = state.Sum(x => x.POS);
                double linkage = 0;
                if (pos != 0)
                    linkage = Math.Round(100 * 1.0 * tx_new / pos, 0);

                pos_state.Add(new
                {
                    y = pos,
                    name = state.Key,
                    drilldown = state.Key
                });
                tx_new_state.Add(new
                {
                    y = tx_new,
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                linkage_state.Add(new
                {
                    y = linkage,
                    name = state.Key,
                    drilldown = state.Key + "  "
                });

                var lga_pos = new List<dynamic>();
                var lga_tx_new = new List<dynamic>();
                var lga_linkage = new List<dynamic>();

                var lgas = new List<string>();
                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    lgas.Add(lga.Key);

                    var tx_new_l = lga.Where(x => x.Row_Number == 1).Sum(x => x.Tx_NEW);
                    var pos_l = lga.Sum(x => x.POS);
                    double linkage_l = 0;
                    if (pos_l != 0)
                        linkage_l = Math.Round(100 * 1.0 * tx_new_l / pos_l, 0);

                    lga_pos.Add(new
                    {
                        y = pos_l,
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_tx_new.Add(new
                    {
                        y = tx_new_l,
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_linkage.Add(new
                    {
                        y = linkage_l,
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });

                    var facility_tx_new = new List<dynamic>();
                    var facility_pos = new List<dynamic>();
                    var facility_linkage = new List<dynamic>();
                    var facilities = new List<string>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facilities.Add(fty.Key);
                        var tx_new_f = fty.Where(x => x.Row_Number == 1).Sum(x => x.Tx_NEW);
                        var pos_f = fty.Sum(x => x.POS);
                        double linkage_f = 0;
                        if (pos_f != 0)
                            linkage_f = Math.Round(100 * 1.0 * tx_new_f / pos_f, 0);

                        facility_tx_new.Add(new List<dynamic>
                        {
                            fty.Key,
                            tx_new_f
                        });
                        facility_pos.Add(new List<dynamic>
                        {
                            fty.Key,
                           pos_f
                        });
                        facility_linkage.Add(new List<dynamic>
                        {
                            fty.Key,
                            linkage_f,
                        });
                    }

                    //add facility
                    lga_drill_down.Add(new { name = "Positives", id = lga.Key, data = facility_pos, type = "column", categories = facilities });
                    lga_drill_down.Add(new { name = "Tx_New", id = lga.Key + " ", data = facility_tx_new, type = "column", categories = facilities });
                    lga_drill_down.Add(new { name = "Linkage (%)", yAxis = 1, type = "scatter", id = lga.Key + "  ", data = facility_linkage, categories = facilities });
                }
                //add lga
                lga_drill_down.Add(new { name = "Positives", id = state.Key, data = lga_pos, categories = lgas, type = "column" });
                lga_drill_down.Add(new { name = "Tx_New", id = state.Key + " ", data = lga_tx_new, categories = lgas, type = "column" });
                lga_drill_down.Add(new { name = "Linkage (%)", yAxis = 1, type = "scatter", id = state.Key + "  ", data = lga_linkage, categories = lgas });

            }

            List<dynamic> state_data = new List<dynamic>
            {
                new
                {
                    name = "Positives",
                    type  = "column",
                    data = pos_state,
                },
                new
                {
                    name = "Tx_New",
                    type  = "column",
                    data = tx_new_state
                },
                new
                {
                    name = "Linkage (%)",
                    data = linkage_state,
                    yAxis = 1,
                    type = "scatter",
                },
            };

            return new
            {
                state_data,
                lga_drill_down,
                states = groupedData.Select(x => x.Key)
            };

        }

        public dynamic GenerateTx_Ret_And_Viral_Load(DataTable dt)
        {
            List<TX_RET> lst = Utilities.ConvertToList<TX_RET>(dt);
            var groupedData = lst.GroupBy(x => x.State);

            var state_ret_den = new List<dynamic>();
            var state_ret_num = new List<dynamic>();
            var state_percent_ret = new List<dynamic>();

            var state_vl_den = new List<dynamic>();
            var state_vl_num = new List<dynamic>();
            var state_percent_vl = new List<dynamic>();

            var lga_drill_down_vl = new List<dynamic>();
            var lga_drill_down_ret = new List<dynamic>();

            foreach (var state in groupedData)
            {
                //retention
                var den_ret = state.Where(x => x.IndicatorType == "Tx_RET")
                    .Sum(x => x.Denominator);
                var num_ret = state.Where(x => x.IndicatorType == "Tx_RET")
                    .Sum(x => x.Numerator);
                double percnt_ret = 0;
                if ((num_ret + den_ret) != 0)
                    percnt_ret = Math.Round(100 * 1.0 * num_ret / (den_ret), 2);

                state_ret_den.Add(new
                {
                    y = den_ret,
                    name = state.Key,
                    drilldown = state.Key
                });
                state_ret_num.Add(new
                {
                    y = num_ret,
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                state_percent_ret.Add(new
                {
                    y = percnt_ret,
                    name = state.Key,
                    drilldown = state.Key + "  "
                });

                //viral load
                var den_vl = state.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Denominator);
                var num_vl = state.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Numerator);
                double percnt_vl = 0;
                if ((num_vl + den_vl) != 0)
                    percnt_vl = Math.Round(100 * 1.0 * num_vl / (den_vl), 2);

                state_vl_den.Add(new
                {
                    y = den_vl,
                    name = state.Key,
                    drilldown = state.Key
                });
                state_vl_num.Add(new
                {
                    y = num_vl,
                    name = state.Key,
                    drilldown = state.Key + " "
                });
                state_percent_vl.Add(new
                {
                    y = percnt_vl,
                    name = state.Key,
                    drilldown = state.Key + "  "
                });


                var lga_den_ret = new List<dynamic>();
                var lga_num_ret = new List<dynamic>();
                var lga_percent_ret = new List<dynamic>();

                var lga_den_vl = new List<dynamic>();
                var lga_num_vl = new List<dynamic>();
                var lga_percent_vl = new List<dynamic>();

                var lgas = new List<string>();
                foreach (var lga in state.GroupBy(x => x.LGA))
                {
                    lgas.Add(lga.Key);

                    //retention
                    var den_ret_l = lga.Where(x => x.IndicatorType == "Tx_RET")
                        .Sum(x => x.Denominator);
                    var num_ret_l = lga.Where(x => x.IndicatorType == "Tx_RET")
                        .Sum(x => x.Numerator);
                    double percnt_ret_l = 0;
                    if ((num_ret_l + den_ret_l) != 0)
                        percnt_ret_l = Math.Round(100 * 1.0 * num_ret_l / (den_ret_l), 0);

                    lga_den_ret.Add(new
                    {
                        y = den_ret_l,
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_num_ret.Add(new
                    {
                        y = num_ret_l,
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_percent_ret.Add(new
                    {
                        y = percnt_ret_l,
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });

                    //viral load
                    var den_vl_l = lga.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Denominator);
                    var num_vl_l = lga.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Numerator);
                    double percnt_vl_l = 0;
                    if ((num_vl_l + den_vl_l) != 0)
                        percnt_vl_l = Math.Round(100 * 1.0 * num_vl_l / (den_vl_l), 0);

                    lga_den_vl.Add(new
                    {
                        y = den_vl_l,
                        name = lga.Key,
                        drilldown = lga.Key
                    });
                    lga_num_vl.Add(new
                    {
                        y = num_vl_l,
                        name = lga.Key,
                        drilldown = lga.Key + " "
                    });
                    lga_percent_vl.Add(new
                    {
                        y = percnt_vl_l,
                        name = lga.Key,
                        drilldown = lga.Key + "  "
                    });


                    var facility_den_ret = new List<dynamic>();
                    var facility_num_ret = new List<dynamic>();
                    var facility_percet_ret = new List<dynamic>();

                    var facility_den_vl = new List<dynamic>();
                    var facility_num_vl = new List<dynamic>();
                    var facility_percet_vl = new List<dynamic>();

                    var facilities = new List<string>();

                    foreach (var fty in lga.GroupBy(x => x.Facility))
                    {
                        facilities.Add(fty.Key);

                        var den_ret_f = fty.Where(x => x.IndicatorType == "Tx_RET")
                        .Sum(x => x.Denominator);
                        var num_ret_f = fty.Where(x => x.IndicatorType == "Tx_RET")
                            .Sum(x => x.Numerator);
                        double percnt_ret_f = 0;
                        if ((num_ret_f + den_ret_f) != 0)
                            percnt_ret_f = Math.Round(100 * 1.0 * num_ret_f / (den_ret_f), 0);

                        facility_den_ret.Add(new List<dynamic>
                        {
                            fty.Key,
                            den_ret_f
                        });
                        facility_num_ret.Add(new List<dynamic>
                        {
                            fty.Key,
                           num_ret_f
                        });
                        facility_percet_ret.Add(new List<dynamic>
                        {
                            fty.Key,
                            percnt_ret_f,
                        });

                        //viral load
                        var den_vl_f = fty.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Denominator);
                        var num_vl_f = fty.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Numerator);
                        double percnt_vl_f = 0;
                        if ((num_vl_f + den_vl_f) != 0)
                            percnt_vl_f = Math.Round(100 * 1.0 * num_vl_f / (den_vl_f), 0);


                        facility_den_vl.Add(new List<dynamic>
                        {
                            fty.Key,
                            den_vl_f
                        });
                        facility_num_vl.Add(new List<dynamic>
                        {
                            fty.Key,
                           num_vl_f
                        });
                        facility_percet_vl.Add(new List<dynamic>
                        {
                            fty.Key,
                            percnt_vl_f,
                        });
                    }

                    //add facility ret
                    lga_drill_down_ret.Add(new { name = "RET (den)", id = lga.Key, data = facility_den_ret, type = "column", categories = facilities });
                    lga_drill_down_ret.Add(new { name = "RET (num)", id = lga.Key + " ", data = facility_num_ret, type = "column", categories = facilities });
                    lga_drill_down_ret.Add(new { name = "Retention (%)", yAxis = 1, type = "scatter", id = lga.Key + "  ", data = facility_percet_ret, categories = facilities });

                    //add facility VL
                    lga_drill_down_vl.Add(new { name = "VLA (den)", id = lga.Key, data = facility_den_vl, type = "column", categories = facilities });
                    lga_drill_down_vl.Add(new { name = "VLA (num)", id = lga.Key + " ", data = facility_num_vl, type = "column", categories = facilities });
                    lga_drill_down_vl.Add(new { name = "VLA Uptake (%)", yAxis = 1, type = "scatter", id = lga.Key + "  ", data = facility_percet_vl, categories = facilities });

                }
                //add lga ret
                lga_drill_down_ret.Add(new { name = "RET (den)", id = state.Key, data = lga_den_ret, categories = lgas, type = "column" });
                lga_drill_down_ret.Add(new { name = "RET (num)", id = state.Key + " ", data = lga_num_ret, categories = lgas, type = "column" });
                lga_drill_down_ret.Add(new { name = "Retention (%)", yAxis = 1, type = "scatter", id = state.Key + "  ", data = lga_percent_ret, categories = lgas });

                //add lga vl
                lga_drill_down_vl.Add(new { name = "VLA (den)", id = state.Key, data = lga_den_vl, categories = lgas, type = "column" });
                lga_drill_down_vl.Add(new { name = "VLA (num)", id = state.Key + " ", data = lga_num_vl, categories = lgas, type = "column" });
                lga_drill_down_vl.Add(new { name = "VLA Uptake (%)", yAxis = 1, type = "scatter", id = state.Key + "  ", data = lga_percent_vl, categories = lgas });
            }

            List<dynamic> state_data_ret = new List<dynamic>
            {
                new
                {
                    name = "RET (den)",
                    type  = "column",
                    data = state_ret_den,
                },
                new
                {
                    name = "RET (num)",
                    type  = "column",
                    data = state_ret_num
                },
                new
                {
                    name = "Retention (%)",
                    data = state_percent_ret,
                    yAxis = 1,
                    type = "scatter",
                },
            };

            List<dynamic> state_data_vl = new List<dynamic>
            {
                new
                {
                    name = "VLA (den)",
                    type  = "column",
                    data = state_vl_den,
                },
                new
                {
                    name = "VLA (num)",
                    type  = "column",
                    data = state_vl_num
                },
                new
                {
                    name = "VLA Uptake (%)",
                    data = state_percent_vl,
                    yAxis = 1,
                    type = "scatter",
                },
            };

            return new
            {
                state_data_ret,
                lga_drill_down_ret,
                state_data_vl,
                lga_drill_down_vl,
                states = groupedData.Select(x => x.Key)
            };

        }

        public dynamic GenerateHTSOtherPITC(DataTable dt)
        {
            List<HTSOtherPITCModel> lst = Utilities.ConvertToList<HTSOtherPITCModel>(dt);
            var groupedData = lst.GroupBy(x => x.SDP);

            var _data_all = new List<dynamic>();

            foreach (var sdp in groupedData)
            {
                var pos = sdp.Sum(x => x.POS);
                var neg = sdp.Sum(x => x.NEG);
                double yd = 0;

                if ((pos + neg) > 0)
                    yd = Math.Round(100 * 1.0 * pos / (pos + neg), 2);

                _data_all.Add(new
                {
                    positives = pos,
                    yield = yd,
                    SDP = sdp.Key,
                    tested = pos + neg
                });
            }
            _data_all = _data_all.OrderByDescending(t => t.tested).ToList();
            return new
            {
                tested = _data_all.Select(s => s.tested),
                positives = _data_all.Select(p => p.positives),
                yield = _data_all.Select(y => y.yield),
                SDPs = _data_all.Select(s => s.SDP)
            };
        }

        public dynamic DrillDownBubbleData(DataTable dt)
        {
            List<HTSIndexViewDataModel> lst = Utilities.ConvertToList<HTSIndexViewDataModel>(dt);

            List<dynamic> state_data = new List<dynamic>();
            List<dynamic> lga_drill_down_data = new List<dynamic>();

            foreach (var gp in lst.GroupBy(x => x.State))
            {
                var total_tested = gp.Sum(p => p.POS) + gp.Sum(p => p.NEG);
                double yield = 0;
                if (total_tested > 0)
                    yield = Math.Round((100 * 1.0 * gp.Sum(s => s.POS) / total_tested), 0);

                state_data.Add(new
                {
                    name = gp.Key,
                    x = total_tested,
                    z = gp.Sum(s => s.POS),
                    y = yield,
                    drilldown = gp.Key
                });
                List<dynamic> lga_data = new List<dynamic>();


                foreach (var lgp in gp.ToList().GroupBy(x => x.LGA))
                {
                    var total_tested_lga = lgp.Sum(p => p.POS) + lgp.Sum(p => p.NEG);
                    double yield_lga = 0;
                    if (total_tested_lga > 0)
                        yield_lga = Math.Round((100 * 1.0 * lgp.Sum(s => s.POS) / total_tested_lga), 0);

                    lga_data.Add(new
                    {
                        name = lgp.Key,
                        drilldown = lgp.Key,
                        x = total_tested_lga,
                        z = lgp.Sum(s => s.POS),
                        y = yield_lga,
                    });

                    List<dynamic> facility_data = new List<dynamic>();

                    foreach (var fty in lgp.ToList().GroupBy(x => x.Facility))
                    {
                        var total_tested_facility = fty.Sum(p => p.POS) + fty.Sum(p => p.NEG);
                        double yield_facility = 0;
                        if (total_tested_facility > 0)
                            yield_facility = Math.Round((100 * 1.0 * fty.Sum(s => s.POS) / total_tested_facility), 0);

                        facility_data.Add(new
                        {
                            name = fty.Key,
                            x = total_tested_facility,
                            z = fty.Sum(s => s.POS),
                            y = yield_facility,
                        });
                    }
                    lga_drill_down_data.Add(new { id = lgp.Key, data = facility_data });
                }
                lga_drill_down_data.Add(new { name = gp.Key, id = gp.Key, data = lga_data });
            }

            return new { state_data, lga_drill_down_data };
        }



        public string ConvertDataTabletoString(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return JsonConvert.SerializeObject(rows);
        }


        public Dictionary<string, int> IndexPeriods { get; set; }

        public Dictionary<string, int> GSMIndexPeriods
        {
            get
            {
                return new Dictionary<string, int>
                {
                    { "28-Dec-2018",1 },
                    { "14-Jan-2019",2 },
                    { "28-Jan-2019",3 },
                    { "11-Feb-2019",4 },
                    { "25-Feb-2019",5 },
                    { "11-Mar-2019",6 },
                    { "25-Mar-2019",7 },
                    { "8-Apr-2019",8 },
                    { "22-Apr-2019",9 },
                    { "6-May-2019",10 },
                    { "20-May-2019",11},
                    { "3-Jun-2019",12 },
                    { "17-Jun-2019",13 }
                };
            }
        }

    }


}