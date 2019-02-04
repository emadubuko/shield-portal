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

            var submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "",  MPM.DAL.DTO.ReportLevel.IP);

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
            return View(ims);
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
                gsm.Add(new
                {
                    ReportingPeriod = item.Key,
                    percent = Math.Round(100 * 1.0 * item.Count() / 20, 0)
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
                int no_of_facilities = item.DistinctBy(x => x.FacilityId).DistinctBy(x => x.Indicator).Count();
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
                int no_of_facilities = item.DistinctBy(x => x.FacilityId).DistinctBy(x => x.Indicator).Count();
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
                return Json("<span style='color: red;'> no file uploaded</span>");

            var filePath = directory + DateTime.Now.ToString("dd MMM yyyy") + "_" + Request.Files[0].FileName;
            Request.Files[0].SaveAs(filePath);

            try
            {
                new TemplateProcessor().ReadFile(Request.Files[0].InputStream, loggedinProfile, filePath);
                return Json("Uploaded succesfully");
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


        //PMTCT_Cascade_ViewModel
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
                    { "14-Feb-2019",4 },
                    { "28-Feb-2019",5 },
                    { "14-Mar-2019",6 },
                    { "28-Mar-2019",7 },
                    { "14-Apr-2019",8 },
                    { "28-Apr-2019",9 },
                    { "14-May-2019",10 },
                    { "28-May-2019",11},
                    { "14-Jun-2019",12 },
                    { "28-Jun-2019",13 }
                };
            }
        }

    }


}