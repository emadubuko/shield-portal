using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using MPM.DAL.DAO;
using MPM.DAL.DTO;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MPM.DAL.Processor
{
    public class TemplateProcessor
    {
        //TB HIV
        public void ReadFile(Stream file, Profile loggedInProfile)
        {
            var sites = new HealthFacilityDAO().RetrieveAll().ToDictionary(x => x.FacilityCode);

            using (ExcelPackage package = new ExcelPackage(file))
            {
                var hts_index_sheet = package.Workbook.Worksheets["HTS_Index"];
                var linkage_sheet = package.Workbook.Worksheets["LInkage to Treatment"];
                var pitc_sheet = package.Workbook.Worksheets["HTS_Other PITC"];
                var pmtct_sheet = package.Workbook.Worksheets["PMTCT"];
                var art_sheet = package.Workbook.Worksheets["ART"];
                var pmtct_viral_load_sheet = package.Workbook.Worksheets["PMTCT_Viral Load"];
                var tb_hiv_sheet = package.Workbook.Worksheets["TB_HIV Cascade"];
                var mainWorksheet = package.Workbook.Worksheets["Home"];

                var period = ExcelHelper.ReadCellText(mainWorksheet, 17, 12);
                if (string.IsNullOrEmpty(period) || string.IsNullOrEmpty(period.Trim()))
                {
                    period = "Jul-18";
                }
                string ReportLevelValue = ExcelHelper.ReadCellText(mainWorksheet, 17, 8);
                string ReportLevel = ExcelHelper.ReadCellText(mainWorksheet, 17, 7);

                var ip = mainWorksheet.Cells["E17"].Value.ToString();
                var state = mainWorksheet.Cells["H17"].Value.ToString();

                var dao = new MPMDAO();
                var mt = new MetaData();
                var previously = dao.GenerateIPUploadReports(loggedInProfile.Organization.Id, period, ReportLevelValue, DTO.ReportLevel.State);

                if (previously == null || previously.Count == 0)
                {
                    mt = new MetaData
                    {
                        DateUploaded = DateTime.Now,
                        IP = loggedInProfile.Organization,
                        ReportingPeriod = period,
                        UploadedBy = loggedInProfile,
                        ReportLevel = DTO.ReportLevel.State,
                        ReportLevelValue = ReportLevelValue
                    };
                }
                else
                {
                    throw new ApplicationException("Report already exist");
                    //mt = dao.Retrieve(previously.FirstOrDefault().Id);
                    //mt.DateUploaded = DateTime.Now;
                    //mt.UploadedBy = loggedInProfile;
                    //mt.ReportLevel = DTO.ReportLevel.State;
                    //mt.ReportLevelValue = ReportLevelValue;
                }


                try
                {
                    List<HTS_Index> hTS_Index_list = RetrieveHTS_data(hts_index_sheet, mt, sites);
                    List<LinkageToTreatment> linkage_list = RetrieveLinkageToTx_data(linkage_sheet, mt, sites);
                    List<ART> art_list = RetrieveART_data(art_sheet, mt, sites);
                    List<PMTCT_Viral_Load> pmtct_viral_load_list = RetrievePMTCTViralLoad_data(pmtct_viral_load_sheet, mt, sites);
                    List<HTS_Other_PITC> pitc_list = RetrievePITC_data(pitc_sheet, mt, sites);

                    List<PMTCT_EID> eid_list = null;
                    List<PMTCT> pmtct_list = RetrievePMTCT_data(pmtct_sheet, mt, sites, out eid_list);

                    var tb_eligible_list = new List<TB_TPT_Eligible>();
                    var tB_Screened = new List<TB_Screened>();
                    var tB_presumptive = new List<TB_Presumptive>();
                    var tb_diagnosed = new List<TB_Presumptive_Diagnosed>();
                    var tB_completed = new List<TB_TPT_Completed>();
                    List<TB_HIV_Treatment> tB_HIV_Treatments = RetrieveTB_HIV_data(tb_hiv_sheet, mt, sites, out tb_eligible_list, out tB_Screened, out tB_presumptive, out tb_diagnosed, out tB_completed);

                    mt.HTS_Index = hTS_Index_list;
                    mt.LinkageToTreatment = linkage_list;
                    mt.ART = art_list;
                    mt.PITC = pitc_list;
                    mt.PMTCT = pmtct_list;
                    mt.Pmtct_Viral_Load = pmtct_viral_load_list;
                    mt.PMTCT_EID = eid_list;

                    //start from here 
                    mt.TB_Screened = tB_Screened;
                    mt.TB_HIV_Treatment = tB_HIV_Treatments;
                    mt.TB_Presumptives = tB_presumptive;
                    mt.TB_Presumptives_Diagnosis = tb_diagnosed;
                    mt.TB_TPT_Completed = tB_completed;
                    mt.TB_TPT_Eligible = tb_eligible_list;

                    dao.BulkInsertWithStatelessSession(mt);
                    //if (previously == null || previously.Count == 0)
                    //{
                        
                    //}
                    //else
                    //{
                    //    dao.UpdateRecord(mt);
                    //}
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        List<TB_HIV_Treatment> RetrieveTB_HIV_data(ExcelWorksheet tb_sheet, MetaData mt, Dictionary<string, HealthFacility> sites, out List<TB_TPT_Eligible> tb_eligible_list, out List<TB_Screened> tB_Screened, out List<TB_Presumptive> tB_presumptive, out List<TB_Presumptive_Diagnosed> tb_diagnosed, out List<TB_TPT_Completed> tB_completed)
        {
            List<TB_HIV_Treatment> tb_tx_list = new List<TB_HIV_Treatment>();
            tb_eligible_list = new List<TB_TPT_Eligible>();
            tB_Screened = new List<TB_Screened>();
            tB_presumptive = new List<TB_Presumptive>();
            tb_diagnosed = new List<TB_Presumptive_Diagnosed>();
            tB_completed = new List<TB_TPT_Completed>();

            int row = 4;
            while (true)
            {
                var facCell = tb_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)//goto Loop;
                        throw new ApplicationException("Invalid facility uploaded");

                    //screnning
                    for (int column = 5; column <= 16; column += 1)
                    {
                        var female_count = tb_sheet.Cells[row, column].Value;
                        var male_count = tb_sheet.Cells[row + 1, column].Value;
                        var AgeGroup = tb_sheet.Cells[3, column].Value.ToString();
                        var Description = tb_sheet.Cells["E2"].Value.ToString();
                        if (female_count != null)
                        {
                            tB_Screened.Add(new TB_Screened
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.F,
                                Description = Description,
                                Number = female_count.ToInt(),
                                MetaData = mt
                            });
                        }
                        if (male_count != null)
                        {
                            tB_Screened.Add(new TB_Screened
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.M,
                                Description = Description,
                                Number = male_count.ToInt(),
                                MetaData = mt
                            });
                        }
                    }


                    //TB_Presumptive
                    for (int column = 18; column <= 29; column += 1)
                    {
                        var female_count = tb_sheet.Cells[row, column].Value;
                        var male_count = tb_sheet.Cells[row + 1, column].Value;
                        var AgeGroup = tb_sheet.Cells[3, column].Value.ToString();
                        var Description = tb_sheet.Cells["R2"].Value.ToString();
                        if (female_count != null)
                        {
                            tB_presumptive.Add(new TB_Presumptive
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.F,
                                Description = Description,
                                Number = female_count.ToInt(),
                                MetaData = mt
                            });
                        }
                        if (male_count != null)
                        {
                            tB_presumptive.Add(new TB_Presumptive
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.M,
                                Description = Description,
                                Number = male_count.ToInt(),
                                MetaData = mt
                            });
                        }
                    }


                    //TB_Presumptive_Diagnosed
                    for (int column = 31; column <= 42; column += 1)
                    {
                        var female_count = tb_sheet.Cells[row, column].Value;
                        var male_count = tb_sheet.Cells[row + 1, column].Value;
                        var AgeGroup = tb_sheet.Cells[3, column].Value.ToString();
                        var Description = tb_sheet.Cells["AE2"].Value.ToString();
                        if (female_count != null)
                        {
                            tb_diagnosed.Add(new TB_Presumptive_Diagnosed
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.F,
                                Description = Description,
                                Number = female_count.ToInt(),
                                MetaData = mt
                            });
                        }
                        if (male_count != null)
                        {
                            tb_diagnosed.Add(new TB_Presumptive_Diagnosed
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.M,
                                Description = Description,
                                Number = male_count.ToInt(),
                                MetaData = mt
                            });
                        }
                    }

                    //TB_TPT_Completed
                    for (int column = 83; column <= 94; column += 1)
                    {
                        var female_count = tb_sheet.Cells[row, column].Value;
                        var male_count = tb_sheet.Cells[row + 1, column].Value;
                        var AgeGroup = tb_sheet.Cells[3, column].Value.ToString();
                        var Description = tb_sheet.Cells["CE2"].Value.ToString();
                        if (female_count != null)
                        {
                            tB_completed.Add(new TB_TPT_Completed
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.F,
                                Description = Description,
                                Number = female_count.ToInt(),
                                MetaData = mt
                            });
                        }
                        if (male_count != null)
                        {
                            tB_completed.Add(new TB_TPT_Completed
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.M,
                                Description = Description,
                                Number = male_count.ToInt(),
                                MetaData = mt
                            });
                        }
                    }

                    //TB_TPT_Eligible
                    for (int column = 44; column <= 77; column += 3)
                    {
                        var AgeGroup = tb_sheet.Cells[2, column].Value.ToString();
                        var Description = tb_sheet.Cells["AR1"].Value.ToString();

                        var plhiv_eligible_female = tb_sheet.Cells[row, column].Value;
                        var started_tpt_female = tb_sheet.Cells[row, column + 1].Value;

                        var plhiv_eligible_male = tb_sheet.Cells[row + 1, column].Value;
                        var started_tpt_male = tb_sheet.Cells[row + 1, column + 1].Value;

                        if (plhiv_eligible_female != null || started_tpt_female != null)
                        {
                            tb_eligible_list.Add(new TB_TPT_Eligible
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.F,
                                Description = Description,
                                PLHIV_eligible_for_TPT = plhiv_eligible_female.ToInt(),
                                Started_on_TPT = started_tpt_female.ToInt(),
                                MetaData = mt
                            });
                        }
                        if (plhiv_eligible_male != null || started_tpt_male != null)
                        {
                            tb_eligible_list.Add(new TB_TPT_Eligible
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.M,
                                Description = Description,
                                PLHIV_eligible_for_TPT = plhiv_eligible_male.ToInt(),
                                Started_on_TPT = started_tpt_male.ToInt(),
                                MetaData = mt
                            });
                        }
                    }


                    //TB_HIV_Treatment
                    for (int column = 96; column <= 129; column += 3)
                    {
                        var AgeGroup = tb_sheet.Cells[2, column].Value.ToString();
                        var Description = tb_sheet.Cells["CR1"].Value.ToString();

                        var new_tb_female = tb_sheet.Cells[row, column].Value;
                        var tx_tb_female = tb_sheet.Cells[row, column + 1].Value;

                        var new_tb_male = tb_sheet.Cells[row + 1, column].Value;
                        var tx_tb_male = tb_sheet.Cells[row + 1, column + 1].Value;

                        if (new_tb_female != null || tx_tb_female != null)
                        {
                            tb_tx_list.Add(new TB_HIV_Treatment
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.F,
                                Description = Description,
                                New_TB_Cases = new_tb_female.ToInt(),
                                Tx_TB = tx_tb_female.ToInt(),
                                MetaData = mt
                            });
                        }
                        if (new_tb_male != null || tx_tb_male != null)
                        {
                            tb_tx_list.Add(new TB_HIV_Treatment
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                Sex = Sex.M,
                                Description = Description,
                                Tx_TB = tx_tb_male.ToInt(),
                                New_TB_Cases = new_tb_male.ToInt(),
                                MetaData = mt
                            });
                        }
                    }

                }
                //Loop: row += 3;
                row++;
            }
            return tb_tx_list;
        }

        List<PMTCT> RetrievePMTCT_data(ExcelWorksheet pmtct_sheet, MetaData mt, Dictionary<string, HealthFacility> sites, out List<PMTCT_EID> eid_list)
        {
            List<PMTCT> pmtct_list = new List<PMTCT>();
            eid_list = new List<PMTCT_EID>();
            int row = 5;
            while (true)
            {
                var facCell = pmtct_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)//goto Loop;
                        throw new ApplicationException("Invalid facility uploaded");

                    for (int column = 4; column <= 23; column += 2)
                    {
                        var new_ANC_client = pmtct_sheet.Cells[row, column].Value;
                        var newly_tested = pmtct_sheet.Cells[row, column + 1].Value;

                        var KnownHIVPos = pmtct_sheet.Cells[row, column + 23].Value;
                        var NewHIVPos = pmtct_sheet.Cells[row, column + 24].Value;
                        var AlreadyOnART = pmtct_sheet.Cells[row, column + 25].Value;
                        var NewOnART = pmtct_sheet.Cells[row, column + 26].Value;

                        var AgeGroup = pmtct_sheet.Cells[3, column].Value.ToString();
                        if (new_ANC_client != null || newly_tested != null
                            || KnownHIVPos != null || NewHIVPos != null
                            || AlreadyOnART != null || NewOnART != null) //ignore if both values are empty
                        {
                            pmtct_list.Add(new PMTCT
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                NewClient = new_ANC_client.ToInt(),
                                NewlyTested = newly_tested.ToInt(),
                                KnownHIVPos = KnownHIVPos.ToInt(),
                                NewHIVPos = NewHIVPos.ToInt(),
                                AlreadyOnART = AlreadyOnART.ToInt(),
                                NewOnART = NewOnART.ToInt(),
                                MetaData = mt
                            });
                        }
                    }

                    for (int column = 73; column <= 78; column += 3)
                    {
                        var sample_collected = pmtct_sheet.Cells[row, column].Value;
                        var pos = pmtct_sheet.Cells[row, column + 1].Value;
                        var artInitiation = pmtct_sheet.Cells[row, column + 2].Value;

                        var AgeGroup = pmtct_sheet.Cells[3, column].Value.ToString();

                        if (sample_collected != null || pos != null || artInitiation != null) //ignore if both values are empty
                        {
                            eid_list.Add(new PMTCT_EID
                            {
                                Site = site,
                                AgeGroup = AgeGroup,
                                ARTInitiation = artInitiation.ToInt(),
                                Pos = pos.ToInt(),
                                SampleCollected = sample_collected.ToInt(),
                                MetaData = mt,
                            });
                        }
                    }
                }
                //Loop: row += 3;
                row++;
            }
            return pmtct_list;
        }



        /*
        private List<PMTCT> RetrievePMTCT_data(ExcelWorksheet pmtct_sheet, MetaData mt, Dictionary<string, HealthFacility> sites)
        {
            List<PMTCT> _list = new List<PMTCT>();
            int row = 5;
            while (true)
            {
                var facCell = pmtct_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)//goto Loop;
                        throw new ApplicationException("Invalid facility uploaded");

                    var new_ANC_client = Convert.ToString(pmtct_sheet.Cells[row, 4].Value);
                    var newly_tested = Convert.ToString(pmtct_sheet.Cells[row, 5].Value);

                    var newHivpos = Convert.ToString(pmtct_sheet.Cells[row, 4].Value);
                    var newArt = Convert.ToString(pmtct_sheet.Cells[row, 5].Value);
                    var knownPos = Convert.ToString(pmtct_sheet.Cells[row, 7].Value);
                    var alreadyOnArt = Convert.ToString(pmtct_sheet.Cells[row, 8].Value);

                    var sampleCollected_0_2 = Convert.ToString(pmtct_sheet.Cells[row, 10].Value);
                    var pos_0_2 = Convert.ToString(pmtct_sheet.Cells[row, 11].Value);
                    var art_initiation_0_2 = Convert.ToString(pmtct_sheet.Cells[row, 12].Value);
                    var sampleCollected_2_12 = Convert.ToString(pmtct_sheet.Cells[row, 13].Value);
                    var pos_2_12 = Convert.ToString(pmtct_sheet.Cells[row, 14].Value);
                    var art_initiation_2_12 = Convert.ToString(pmtct_sheet.Cells[row, 15].Value);

                    if (!string.IsNullOrEmpty(newHivpos) && !string.IsNullOrEmpty(newArt) && !string.IsNullOrEmpty(knownPos) && !string.IsNullOrEmpty(alreadyOnArt)
                        && !string.IsNullOrEmpty(sampleCollected_0_2) && !string.IsNullOrEmpty(pos_0_2) && !string.IsNullOrEmpty(sampleCollected_2_12) && !string.IsNullOrEmpty(pos_2_12)
                        && !string.IsNullOrEmpty(art_initiation_0_2) && !string.IsNullOrEmpty(art_initiation_2_12)) //ignore if both values are empty
                    {
                        _list.Add(new PMTCT
                        {
                            Site = site,
                            // AgeGroup = pmtct_sheet.Cells[3, 4].Value.ToString(),
                            NewHIVPos = !string.IsNullOrEmpty(newHivpos) ? Convert.ToInt32(newHivpos) : (int?)null,
                            NewOnART = !string.IsNullOrEmpty(newArt) ? Convert.ToInt32(newArt) : (int?)null,
                            KnownHIVPos = !string.IsNullOrEmpty(knownPos) ? Convert.ToInt32(knownPos) : (int?)null,
                            AlreadyOnART = !string.IsNullOrEmpty(alreadyOnArt) ? Convert.ToInt32(alreadyOnArt) : (int?)null,

                            SampleCollected_between_0_to_2 = !string.IsNullOrEmpty(sampleCollected_0_2) ? Convert.ToInt32(sampleCollected_0_2) : (int?)null,
                            Pos_between_0_to_2 = !string.IsNullOrEmpty(pos_0_2) ? Convert.ToInt32(pos_0_2) : (int?)null,
                            ARTInitiation_between_0_to_2 = !string.IsNullOrEmpty(art_initiation_0_2) ? Convert.ToInt32(art_initiation_0_2) : (int?)null,

                            SampleCollected_between_2_to_12 = !string.IsNullOrEmpty(sampleCollected_2_12) ? Convert.ToInt32(sampleCollected_2_12) : (int?)null,
                            Pos_between_2_to_12 = !string.IsNullOrEmpty(pos_2_12) ? Convert.ToInt32(pos_2_12) : (int?)null,
                            ARTInitiation_between_2_to_12 = !string.IsNullOrEmpty(art_initiation_2_12) ? Convert.ToInt32(art_initiation_2_12) : (int?)null,
                            MetaData = mt
                        });
                    }
                }
                //Loop: row += 1;
            }
            return _list;
        }
        */

        private List<HTS_Other_PITC> RetrievePITC_data(ExcelWorksheet pitc_sheet, MetaData mt, Dictionary<string, HealthFacility> sites)
        {
            List<HTS_Other_PITC> _list = new List<HTS_Other_PITC>();
            int row = 5;
            while (true)
            {
                var facCell = pitc_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)
                        goto Loop; ; //throw new ApplicationException("Invalid facility uploaded");


                    int counter = 8;
                    for (int column = 8; ;)
                    {
                        string _sdp = Convert.ToString(pitc_sheet.Cells[2, counter].Value);
                        if (string.IsNullOrEmpty(_sdp))
                            break;

                        var pos = Convert.ToString(pitc_sheet.Cells[row, column].Value);
                        var neg = Convert.ToString(pitc_sheet.Cells[row, column + 1].Value);

                        if (!string.IsNullOrEmpty(pos) && !string.IsNullOrEmpty(neg)) //ignore if both values are empty
                        {
                            //female
                            _list.Add(new HTS_Other_PITC
                            {
                                Site = site,
                                AgeGroup = pitc_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.F,
                                SDP = _sdp,
                                POS = !string.IsNullOrEmpty(pos) ? Convert.ToInt32(pos) : (int?)null,
                                NEG = !string.IsNullOrEmpty(neg) ? Convert.ToInt32(neg) : (int?)null,
                                MetaData = mt
                            });
                        }
                        //male
                        pos = Convert.ToString(pitc_sheet.Cells[row + 1, column].Value);
                        neg = Convert.ToString(pitc_sheet.Cells[row + 1, column + 1].Value);

                        if (!string.IsNullOrEmpty(pos) && !string.IsNullOrEmpty(neg)) //ignore if both values are empty
                        {
                            _list.Add(new HTS_Other_PITC
                            {
                                Site = site,
                                AgeGroup = pitc_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.M,
                                SDP = _sdp,
                                POS = !string.IsNullOrEmpty(pos) ? Convert.ToInt32(pos) : (int?)null,
                                NEG = !string.IsNullOrEmpty(neg) ? Convert.ToInt32(neg) : (int?)null,
                                MetaData = mt
                            });
                        }


                        column += 2;
                        if (column % 28 == 0)
                        {
                            counter += 20;
                        }
                    }
                }
                Loop: row += 3;
            }
            return _list;
        }

        private List<ART> RetrieveART_data(ExcelWorksheet art_sheet, MetaData mt, Dictionary<string, HealthFacility> sites)
        {
            List<ART> art_list = new List<ART>();
            int row = 5;
            while (true)
            {
                var facCell = art_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)
                        goto Loop; ; // throw new ApplicationException("Invalid facility uploaded");


                    for (int column = 5; column <= 73;)
                    {
                        ART_Indicator_Type indicatorType = column < 36 ? ART_Indicator_Type.Tx_RET : ART_Indicator_Type.Tx_VLA;

                        var f_den = art_sheet.Cells[row, column].Value;
                        var f_num = art_sheet.Cells[row, column + 1].Value;

                        if (f_den != null || f_num != null) //ignore if both values are empty
                        {
                            //female
                            art_list.Add(new ART
                            {
                                Site = site,
                                AgeGroup = art_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.F,
                                IndicatorType = indicatorType,
                                Denominator = f_den.ToInt(),
                                Numerator = f_num.ToInt(),
                                MetaData = mt
                            });
                        }


                        //male
                        var m_den = art_sheet.Cells[row + 1, column].Value;
                        var m_num = art_sheet.Cells[row + 1, column + 1].Value;

                        if (m_den != null || m_num != null) //ignore if both values are empty
                        {
                            art_list.Add(new ART
                            {
                                Site = site,
                                AgeGroup = art_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.M,
                                IndicatorType = indicatorType,
                                Denominator = m_den.ToInt(),
                                Numerator = m_num.ToInt(),
                                MetaData = mt
                            });
                        }


                        column += 3;
                        if (column == 38)
                        {
                            column += 3;
                        }
                    }
                }
                Loop: row += 3;
            }
            return art_list;
        }

        List<PMTCT_Viral_Load> RetrievePMTCTViralLoad_data(ExcelWorksheet pmtct_viral_load_sheet, MetaData mt, Dictionary<string, HealthFacility> sites)
        {
            List<PMTCT_Viral_Load> _list = new List<PMTCT_Viral_Load>();
            int row = 5;
            while (true)
            {
                var facCell = pmtct_viral_load_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)
                        goto Loop; // throw new ApplicationException("Invalid facility uploaded");

                    for (int column = 5; column <= 24;)
                    {
                        var less_than_1000 = pmtct_viral_load_sheet.Cells[row, column].Value;
                        var _greater_than_1000 = pmtct_viral_load_sheet.Cells[row, column + 1].Value;

                        if (less_than_1000 != null && _greater_than_1000 != null) //ignore if both values are empty
                        {
                            _list.Add(new PMTCT_Viral_Load
                            {
                                Site = site,
                                AgeGroup = pmtct_viral_load_sheet.Cells[3, column].Value.ToString(),
                                Category = PMTCT_Category.Newly_Identified,
                                _less_than_1000 = less_than_1000.ToInt(), 
                                _greater_than_1000 = _greater_than_1000.ToInt(), 
                                MetaData = mt
                            });
                        }


                        less_than_1000 = pmtct_viral_load_sheet.Cells[row + 1, column].Value;
                        _greater_than_1000 = pmtct_viral_load_sheet.Cells[row + 1, column + 1].Value;

                        if (less_than_1000 != null && _greater_than_1000 != null) //ignore if both values are empty
                        {
                            _list.Add(new PMTCT_Viral_Load
                            {
                                Site = site,
                                AgeGroup = pmtct_viral_load_sheet.Cells[3, column].Value.ToString(),
                                Category = PMTCT_Category.Already_HIV_Positive,
                                _less_than_1000 = less_than_1000.ToInt(),
                                _greater_than_1000 = _greater_than_1000.ToInt(),
                                MetaData = mt
                            });
                        }
                        column += 2;
                    }
                }
                Loop: row += 3;
            }
            return _list;
        }

        List<LinkageToTreatment> RetrieveLinkageToTx_data(ExcelWorksheet linkage_sheet, MetaData mt, Dictionary<string, HealthFacility> sites)
        {
            List<LinkageToTreatment> _list = new List<LinkageToTreatment>();
            int row = 5;
            while (true)
            {
                var facCell = linkage_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)//goto Loop;
                        throw new ApplicationException("Invalid facility uploaded");

                    for (int column = 5; column <= 14; column++)
                    {

                        var f_POS = Convert.ToString(linkage_sheet.Cells[row, column].Value);
                        //var f_TX_NEW = Convert.ToString(linkage_sheet.Cells[row, column + 1].Value);

                        if (!string.IsNullOrEmpty(f_POS)) //ignore if both values are empty
                        {
                            //female
                            _list.Add(new LinkageToTreatment
                            {
                                Site = site,
                                AgeGroup = linkage_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.F,
                                POS = !string.IsNullOrEmpty(f_POS) ? Convert.ToInt32(f_POS) : (int?)null,
                                MetaData = mt
                            });
                        }


                        //male
                        var m_POS = Convert.ToString(linkage_sheet.Cells[row + 1, column].Value); // !string.IsNullOrEmpty() ? Convert.ToInt32(hts_index_sheet.Cells[row + 1, column].Value) : (int?)null;
                        //var m_TX_NEW = Convert.ToString(linkage_sheet.Cells[row + 1, column + 1].Value); // !string.IsNullOrEmpty() ? Convert.ToInt32(hts_index_sheet.Cells[row + 1, column + 1].Value) : (int?)null;

                        if (!string.IsNullOrEmpty(m_POS))// && !string.IsNullOrEmpty(m_TX_NEW)) //ignore if both values are empty
                        {
                            _list.Add(new LinkageToTreatment
                            {
                                Site = site,
                                AgeGroup = linkage_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.M,
                                POS = !string.IsNullOrEmpty(f_POS) ? Convert.ToInt32(m_POS) : (int?)null,
                                //Tx_NEW = !string.IsNullOrEmpty(m_TX_NEW) ? Convert.ToInt32(m_TX_NEW) : (int?)null,
                                MetaData = mt
                            });
                        }

                        //column += 2;
                    }
                }
                row += 3;
            }
            return _list;
        }


        List<HTS_Index> RetrieveHTS_data(ExcelWorksheet hts_index_sheet, MetaData mt, Dictionary<string, HealthFacility> sites)
        {
            List<HTS_Index> hTS_Index_list = new List<HTS_Index>();
            int row = 6;
            while (true)
            {
                var facCell = hts_index_sheet.Cells["C" + row];
                if (facCell.Value == null || string.IsNullOrEmpty(facCell.Value.ToString()))
                {
                    break;
                }
                else
                {
                    sites.TryGetValue(facCell.Value.ToString(), out HealthFacility site);

                    if (site == null)
                        goto Loop; // throw new ApplicationException("Invalid facility uploaded");

                    for (int column = 8; column <= 32;)
                    {
                        string testingtype = column < 17 ? hts_index_sheet.Cells[3, 8].Value.ToString() : hts_index_sheet.Cells[3, 17].Value.ToString();

                        var f_POS = Convert.ToString(hts_index_sheet.Cells[row, column].Value);
                        var f_NEG = Convert.ToString(hts_index_sheet.Cells[row, column + 1].Value);

                        if (!string.IsNullOrEmpty(f_POS) && !string.IsNullOrEmpty(f_NEG)) //ignore if both values are empty
                        {
                            //female
                            hTS_Index_list.Add(new HTS_Index
                            {
                                Site = site,
                                AgeGroup = hts_index_sheet.Cells[4, column].Value.ToString(),
                                Sex = Sex.F,
                                TestingType = testingtype,
                                POS = !string.IsNullOrEmpty(f_POS) ? Convert.ToInt32(f_POS) : (int?)null,
                                NEG = !string.IsNullOrEmpty(f_NEG) ? Convert.ToInt32(f_NEG) : (int?)null,
                                MetaData = mt
                            });
                        }


                        //male
                        var m_POS = Convert.ToString(hts_index_sheet.Cells[row + 1, column].Value); // !string.IsNullOrEmpty() ? Convert.ToInt32(hts_index_sheet.Cells[row + 1, column].Value) : (int?)null;
                        var m_NEG = Convert.ToString(hts_index_sheet.Cells[row + 1, column + 1].Value); // !string.IsNullOrEmpty() ? Convert.ToInt32(hts_index_sheet.Cells[row + 1, column + 1].Value) : (int?)null;

                        if (!string.IsNullOrEmpty(m_POS) && !string.IsNullOrEmpty(m_NEG)) //ignore if both values are empty
                        {
                            hTS_Index_list.Add(new HTS_Index
                            {
                                Site = site,
                                AgeGroup = hts_index_sheet.Cells[4, column].Value.ToString(),
                                Sex = Sex.M,
                                TestingType = testingtype,
                                POS = !string.IsNullOrEmpty(f_POS) ? Convert.ToInt32(m_POS) : (int?)null,
                                NEG = !string.IsNullOrEmpty(f_NEG) ? Convert.ToInt32(m_NEG) : (int?)null,
                                MetaData = mt
                            });
                        }


                        column += 2;
                        if (column == 14)
                        {
                            column += 3;
                        }
                    }
                }
                Loop: row += 3;
            }
            return hTS_Index_list;
        }

        public string PopulateTemplate(Profile loggedinProfile, string state)
        {
            var facilities = new MPMDAO().GetPivotTableFromFacility(loggedinProfile.Organization.ShortName, state);
            string directory = "~/Report/Template/MPM/";
            string fileName = "CDC MPM Tool_" + loggedinProfile.Organization.ShortName + "_" + state + ".xlsm";

            string template = System.Web.Hosting.HostingEnvironment.MapPath(directory + "GSM_Template.xlsm");


            using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
            {
                var homesheet = package.Workbook.Worksheets["Home"];
                homesheet.Cells["E17"].Value = loggedinProfile.Organization.ShortName;
                homesheet.Cells["H17"].Value = state;

                var hts_index_sheet = package.Workbook.Worksheets["HTS_Index"];
                var linkage_sheet = package.Workbook.Worksheets["LInkage to Treatment"];
                var pitc_sheet = package.Workbook.Worksheets["HTS_Other PITC"];
                var pmtct_sheet = package.Workbook.Worksheets["PMTCT"];
                var art_sheet = package.Workbook.Worksheets["ART"];
                var pmtct_viral_load_sheet = package.Workbook.Worksheets["PMTCT_Viral Load"];
                var TB_HIV_sheet = package.Workbook.Worksheets["TB_HIV Cascade"];

                int row = 6;
                int p_row = 5;
                foreach (var f in facilities.Where(x => x.HTS))
                {
                    hts_index_sheet.Cells["B" + row].Value = f.Facility;
                    hts_index_sheet.Cells["C" + row].Value = f.DATIMCode;

                    linkage_sheet.Cells["B" + p_row].Value = f.Facility;
                    linkage_sheet.Cells["C" + p_row].Value = f.DATIMCode;

                    pitc_sheet.Cells["B" + p_row].Value = f.Facility;
                    pitc_sheet.Cells["C" + p_row].Value = f.DATIMCode;

                    row += 3;
                    p_row += 3;
                }

                row = 5;
                int pmtct_row = 5;
                foreach (var f in facilities.Where(x => x.PMTCT))
                {
                    pmtct_sheet.Cells["B" + row].Value = f.Facility;
                    pmtct_sheet.Cells["C" + row].Value = f.DATIMCode;

                    pmtct_viral_load_sheet.Cells["B" + pmtct_row].Value = f.Facility;
                    pmtct_viral_load_sheet.Cells["C" + pmtct_row].Value = f.DATIMCode;

                    row += 1;
                    pmtct_row += 3;
                }

                row = 5;
                foreach (var f in facilities.Where(x => x.ART))
                {
                    art_sheet.Cells["B" + row].Value = f.Facility;
                    art_sheet.Cells["C" + row].Value = f.DATIMCode;
                    row += 3;
                }

                //if it has any of hts or art, then assume for TB too
                row = 4;
                foreach (var f in facilities.Where(x => x.ART || x.HTS))
                {
                    TB_HIV_sheet.Cells["B" + row].Value = f.Facility;
                    TB_HIV_sheet.Cells["C" + row].Value = f.DATIMCode;
                    row += 3;
                }
                package.SaveAs(new FileInfo(System.Web.Hosting.HostingEnvironment.MapPath(directory + fileName)));
            }

            return fileName;
        }




    }


}
