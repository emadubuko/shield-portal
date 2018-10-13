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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
            var vm = new UploadViewModel();
            vm.IPReports = new Dictionary<string, List<bool>>();
            var mpmDAO = new MPMDAO();

            var submissions = mpmDAO.GenerateIPUploadReports(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0, "", "", null);

            var reports = submissions.GroupBy(x => x.IPName);

            foreach (var iplevel in reports)
            {
                vm.ImplementingPartner = iplevel.Key;
                foreach (var state in iplevel.GroupBy(x => x.ReportingLevelValue))
                {
                    var entries = state.ToList().Select(x => x.ReportPeriod).ToList();
                    List<bool> uploaded = new List<bool>(12);
                    foreach (var index in IndexPeriods.Keys)
                    {
                        if (entries.Any(x => x.Contains(index)))
                            uploaded.Add(true);
                        else
                            uploaded.Add(false);
                    }
                    vm.IPReports.Add(state.Key + "|" + iplevel.Key, uploaded);
                }
            }
            ViewBag.reportPeriod = mpmDAO.GetLastReport(loggedinProfile.RoleName == "ip" ? loggedinProfile.Organization.Id : 0);
            ViewBag.ReportedPeriods = submissions.Select(x => x.ReportPeriod).Distinct();
            return View(vm);
        }

        [HttpPost]
        public string DownloadPreviousReport(string IPState)
        {

            string IP = IPState.Split('|')[1];
            string State = IPState.Split('|')[0];
            var ip = new OrganizationDAO().SearchByShortName(IP);
            string period = "Jul-18";
            var dao = new MPMDAO();
            var previously = dao.GenerateIPUploadReports(ip.Id, period, State, MPM.DAL.DTO.ReportLevel.State);

            if (previously != null && previously.FirstOrDefault() != null)
            {
                return previously.FirstOrDefault().FilePath.Split(new string[] { "\\Report\\" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            return "";
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
                    TB_hiv_tx = dr.Field<string>("TB_HIV_Treatment"),
                    TB_Presumptive = dr.Field<string>("TB_Presumptive"),
                    TB_Presumptive_Diagnosed = dr.Field<string>("TB_Presumptive_Diagnosed"),
                    TPT_Completed = dr.Field<string>("TPT_Completed"),
                    TPT_Eligible = dr.Field<string>("TPT_Eligible"),
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

        public ActionResult Download()
        {
            var facilities = new HealthFacilityDAO().RetrievebyIP(loggedinProfile.Organization.Id);
            List<State> states = new List<State>();
            if (facilities != null)
                states.AddRange(facilities.Select(x => x.LGA.State).Distinct());

            return View(states);
        }

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

        public ActionResult Dashboard(string q = "")
        {
            if (string.IsNullOrEmpty(q))
                return View("iDashboard");
            else
                return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public dynamic RetriveData()
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
            }

            Dictionary<string, dynamic> _data = new Dictionary<string, dynamic>();
            foreach (var sp in storedProcedures)
            {
                var data = dao.RetriveDataAsDataTables(sp.Value, ip);
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
            List<TB_TPT_ViewModel> lst = ConvertToList<TB_TPT_ViewModel>(dt);

            var groupedData = lst.GroupBy(x => x.State);

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
        }


        //tb_treatment
        public dynamic GenerateTB_Treatment(DataTable dt)
        {
            List<TB_Treatment_ViewModel> lst = ConvertToList<TB_Treatment_ViewModel>(dt);

            var groupedData = lst.GroupBy(x => x.State);

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
        }

        //tb_stat
        public dynamic GenerateTB_STAT(DataTable dt)
        {
            List<TB_STAT_ViewModel> lst = ConvertToList<TB_STAT_ViewModel>(dt);

            var groupedData = lst.OrderBy(x => x.State).GroupBy(x => x.State);
            var _data = new List<dynamic>();
            foreach (var state in groupedData)
            {
                _data.Add(new
                {
                    tb_screened = state.Sum(x => x.TB_Screened),
                    tb_presumptive = state.Sum(x => x.TB_Presumptive),
                    tb_bac_diagnosis = state.Sum(x => x.TB_Bacteriology_Diagnosis),
                    tb_diagnosed = state.Sum(x => x.TB_Diagnosed),
                    State = state.Key
                });
            }

            return new
            {
                tb_stat = new List<dynamic>
                {
                    new
                    {
                        name = "Screened",
                        data = _data.Select(x => x.tb_screened)
                    },
                    new
                    {
                        name = "TB presumptive",
                        data = _data.Select(x => x.tb_presumptive)
                    },
                    new
                    {
                        name = "Bacteriologic diagnosis",
                        data = _data.Select(x => x.tb_bac_diagnosis)
                    },
                    new
                    {
                        name = "Diagnosed of Active TB",
                        data = _data.Select(x => x.tb_diagnosed)
                    }
                },
                State = _data.Select(x => x.State)
            };
        }


        //PMTCT_Cascade_ViewModel
        public dynamic GeneratePMTCT_Cascade(DataTable dt)
        {
            List<PMTCT_Cascade_ViewModel> _dt = ConvertToList<PMTCT_Cascade_ViewModel>(dt);

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
                    y = state.Sum(x => x.NewlyTested),
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
                        y = lga.Sum(x => x.NewlyTested),
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
                            fty.Key, fty.Sum(x => x.NewlyTested),
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
                    lga_data_maternal_uptake.Add(new { id = lga.Key, data = facility_data_newClient });
                    lga_data_maternal_uptake.Add(new { id = lga.Key + " ", data = facility_data_knowStatus });

                    //lga_maternal_clinical_cascade
                    lga_maternal_clinical_cascade.Add(new { id = lga.Key, data = hiv_pos_reporting_period_facility });
                    lga_maternal_clinical_cascade.Add(new { id = lga.Key + " ", data = hiv_pos_on_ART_facility });
                    lga_maternal_clinical_cascade.Add(new { id = lga.Key + "  ", data = hiv_exposed_tested_at_2month_facility });
                    lga_maternal_clinical_cascade.Add(new { id = lga.Key + "   ", data = hiv_exposed_tested_at_12mnth_facility });

                    //infant_linkage
                    lga_infant_linkage.Add(new { id = lga.Key, data = facility_eid_art_initiation });
                    lga_infant_linkage.Add(new { id = lga.Key + " ", data = facility_hiv_pos_infant_at_2mnth });
                    lga_infant_linkage.Add(new { id = lga.Key + "  ", data = facility_hiv_pos_infant_at_12mnth });
                }
                lga_data_maternal_uptake.Add(new { name = state.Key, id = state.Key, data = lga_data_newClient });
                lga_data_maternal_uptake.Add(new { name = state.Key + " ", id = state.Key + " ", data = lga_data_knowStatus });

                //lga_maternal_clinical_cascade
                lga_maternal_clinical_cascade.Add(new { name = state.Key, id = state.Key, data = hiv_pos_reporting_period_lga });
                lga_maternal_clinical_cascade.Add(new { name = state.Key, id = state.Key + " ", data = hiv_pos_on_ART_lga });
                lga_maternal_clinical_cascade.Add(new { name = state.Key, id = state.Key + "  ", data = hiv_exposed_tested_at_2month_lga });
                lga_maternal_clinical_cascade.Add(new { name = state.Key, id = state.Key + "   ", data = hiv_exposed_tested_at_12mnth_lga });

                //infant_linkage
                lga_infant_linkage.Add(new { name = state.Key, id = state.Key, data = lga_eid_art_initiation });
                lga_infant_linkage.Add(new { name = state.Key, id = state.Key + " ", data = lga_hiv_pos_infant_at_2mnth });
                lga_infant_linkage.Add(new { name = state.Key, id = state.Key + "  ", data = lga_hiv_pos_infant_at_12mnth });
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
            List<PMTCT_VL_ViewModel> lst = ConvertToList<PMTCT_VL_ViewModel>(dt);

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
                drilldown_series_known_status.Add(new { name = "Not Suppressed", id = state.Key + "_unsuppressed", data = lga_data_already_pos_unsuppressed});
                 
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
            List<LinkageViewModel> lst = ConvertToList<LinkageViewModel>(dt);

            var groupedData = lst.GroupBy(x => x.State);

            var _data = new List<dynamic>();
            foreach (var state in groupedData)
            {
                var tx_new = state.Where(x => x.Row_Number == 1).Sum(x => x.Tx_NEW);
                var pos = state.Sum(x => x.POS);
                double linkage = 0;
                if (pos != 0)
                    linkage = Math.Round(100 * 1.0 * tx_new / pos, 2);

                _data.Add(new
                {
                    Tx_New = tx_new,
                    Linkage = linkage,
                    POS = pos,
                    State = state.Key
                });
            }

            _data = _data.OrderByDescending(t => t.POS).ToList();

            return new
            {
                Tx_New = _data.Select(s => s.Tx_New),
                Linkage = _data.Select(s => s.Linkage),
                POS = _data.Select(y => y.POS),
                State = _data.Select(s => s.State),
            };
        }

        public dynamic GenerateTx_Ret_And_Viral_Load(DataTable dt)
        {
            List<TX_RET> lst = ConvertToList<TX_RET>(dt);

            var groupedData = lst.GroupBy(x => x.State);

            var _data_Ret = new List<dynamic>();
            var _data_VL = new List<dynamic>();
            foreach (var state in groupedData)
            {
                var den_ret = state.Where(x => x.IndicatorType == "Tx_RET").Sum(x => x.Denominator);
                var num_ret = state.Where(x => x.IndicatorType == "Tx_RET").Sum(x => x.Numerator);
                double percnt_ret = 0;
                if ((num_ret + den_ret) != 0)
                    percnt_ret = Math.Round(100 * 1.0 * num_ret / (num_ret + den_ret), 2);

                _data_Ret.Add(new
                {
                    denominator = den_ret,
                    percent = percnt_ret,
                    numerator = num_ret,
                    State = state.Key
                });
                var den_vl = state.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Denominator);
                var num_vl = state.Where(x => x.IndicatorType == "Tx_VLA").Sum(x => x.Numerator);
                double percnt_vl = 0;
                if ((num_vl + den_vl) != 0)
                    percnt_vl = Math.Round(100 * 1.0 * num_ret / (num_vl + den_vl), 2);

                _data_VL.Add(new
                {
                    denominator = den_vl,
                    percent = percnt_vl,
                    numerator = num_vl,
                    State = state.Key
                });
            }

            _data_Ret = _data_Ret.OrderByDescending(t => t.denominator).ToList();
            _data_VL = _data_VL.OrderByDescending(t => t.denominator).ToList();

            return new
            {
                den_ret = _data_Ret.Select(s => s.denominator),
                num_ret = _data_Ret.Select(s => s.numerator),
                percnt_ret = _data_Ret.Select(y => y.percent),
                State_ret = _data_Ret.Select(s => s.State),

                den_vl = _data_VL.Select(s => s.denominator),
                num_vl = _data_VL.Select(s => s.numerator),
                percnt_vl = _data_VL.Select(y => y.percent),
                State_vl = _data_VL.Select(s => s.State)
            };
        }

        public dynamic GenerateHTSOtherPITC(DataTable dt)
        {
            List<HTSOtherPITCModel> lst = ConvertToList<HTSOtherPITCModel>(dt);

            var groupedData = lst.GroupBy(x => x.SDP);

            var _data_all = new List<dynamic>();

            foreach (var sdp in groupedData)
            {
                var pos = sdp.Sum(x => x.POS);
                var neg = sdp.Sum(x => x.NEG);
                var yd = Math.Round(100 * 1.0 * pos / (pos + neg), 2);

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
            List<HTSIndexViewDataModel> lst = ConvertToList<HTSIndexViewDataModel>(dt);

            List<dynamic> state_data = new List<dynamic>();
            List<dynamic> lga_drill_down_data = new List<dynamic>();

            foreach (var gp in lst.GroupBy(x => x.State))
            {
                var total_tested = gp.Sum(p => p.POS) + gp.Sum(p => p.NEG);
                var yield = Math.Round((100 * 1.0 * gp.Sum(s => s.POS) / total_tested), 0);

                state_data.Add(new
                {
                    name = gp.Key,
                    x = gp.Sum(s => s.POS),
                    z = total_tested,
                    y = yield,
                    drilldown = gp.Key
                });
                List<dynamic> lga_data = new List<dynamic>();


                foreach (var lgp in gp.ToList().GroupBy(x => x.LGA))
                {
                    var total_tested_lga = lgp.Sum(p => p.POS) + lgp.Sum(p => p.NEG);
                    var yield_lga = Math.Round((100 * 1.0 * lgp.Sum(s => s.POS) / total_tested_lga), 0);

                    lga_data.Add(new
                    {
                        name = lgp.Key,
                        drilldown = lgp.Key,
                        x = lgp.Sum(s => s.POS),
                        z = total_tested_lga,
                        y = yield_lga,
                    });

                    List<dynamic> facility_data = new List<dynamic>();

                    foreach (var fty in lgp.ToList().GroupBy(x => x.Facility))
                    {
                        var total_tested_facility = fty.Sum(p => p.POS) + fty.Sum(p => p.NEG);
                        var yield_facility = Math.Round((100 * 1.0 * fty.Sum(s => s.POS) / total_tested_facility), 0);

                        facility_data.Add(new
                        {
                            name = fty.Key,
                            x = fty.Sum(s => s.POS),
                            z = total_tested_facility,
                            y = yield_facility,
                        });
                    }
                    lga_drill_down_data.Add(new { id = lgp.Key, data = facility_data });
                }
                lga_drill_down_data.Add(new { name = gp.Key, id = gp.Key, data = lga_data });
            }

            return new { state_data, lga_drill_down_data };
        }

        public List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name))
                    {
                        PropertyInfo pI = objT.GetType().GetProperty(pro.Name);
                        pro.SetValue(objT, row[pro.Name] == DBNull.Value ? null : Convert.ChangeType(row[pro.Name], pI.PropertyType));
                    }
                }
                return objT;
            }).ToList();
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


        public Dictionary<string, int> IndexPeriods

        {
            get
            {
                return new Dictionary<string, int>
                {
                    { "Jan",1 },
                    { "Feb",2 },
                    { "Mar",3 },
                    { "Apr",4 },
                    { "May",5 },
                    { "Jun",6 },
                    { "Jul",7 },
                    { "Aug",8 },
                    { "Sep",9 },
                    { "Oct",10 },
                    { "Nov",11},
                    { "Dec",12 }
                };
            }
        }
    }


}