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
                var mainWorksheet = package.Workbook.Worksheets["Guide"];

                var period = ExcelHelper.ReadCellText(mainWorksheet, 15, 7);

                var dao = new MPMDAO();
                var mt = new MetaData();
                var previously = dao.GenerateIPUploadReports(loggedInProfile.Organization.Id, period);

                if (previously == null || previously.Count == 0)
                {
                    mt = new MetaData
                    {
                        DateUploaded = DateTime.Now,
                        IP = loggedInProfile.Organization,
                        ReportingPeriod = period,
                        UploadedBy = loggedInProfile,
                    };
                }
                else
                {
                    mt = dao.Retrieve(previously.FirstOrDefault().Id);
                    mt.DateUploaded = DateTime.Now;
                    mt.UploadedBy = loggedInProfile;
                }


                try
                {
                    List<HTS_Index> hTS_Index_list = RetrieveHTS_data(hts_index_sheet, mt, sites);
                    List<LinkageToTreatment> linkage_list = RetrieveLinkageToTx_data(linkage_sheet, mt, sites);
                    List<ART> art_list = RetrieveART_data(art_sheet, mt, sites);
                    List<PMTCT_Viral_Load> pmtct_viral_load_list = RetrievePMTCTViralLoad_data(pmtct_viral_load_sheet, mt, sites);
                    List<HTS_Other_PITC> pitc_list = RetrievePITC_data(pitc_sheet, mt, sites);
                    List<PMTCT> pmtct_list = RetrievePMTCT_data(pmtct_sheet, mt, sites);

                    mt.HTS_Index = hTS_Index_list;
                    mt.LinkageToTreatment = linkage_list;
                    mt.ART = art_list;
                    mt.PITC = pitc_list;
                    mt.PMTCT = pmtct_list;
                    mt.Pmtct_Viral_Load = pmtct_viral_load_list;

                    if (previously == null || previously.Count == 0)
                    {                        
                        dao.BulkInsertWithStatelessSession(mt);
                    }
                    else
                    {
                        dao.UpdateRecord(mt);
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


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

                    if (site == null)
                        goto Loop; ; //throw new ApplicationException("Invalid facility uploaded");

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
                Loop: row += 1;
            }
            return _list;
        }

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

                        var f_den = Convert.ToString(art_sheet.Cells[row, column].Value);
                        var f_num = Convert.ToString(art_sheet.Cells[row, column + 1].Value);

                        if (!string.IsNullOrEmpty(f_den) && !string.IsNullOrEmpty(f_num)) //ignore if both values are empty
                        {
                            //female
                            art_list.Add(new ART
                            {
                                Site = site,
                                AgeGroup = art_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.F,
                                IndicatorType = indicatorType,
                                Denominator = !string.IsNullOrEmpty(f_den) ? Convert.ToInt32(f_den) : (int?)null,
                                Numerator = !string.IsNullOrEmpty(f_num) ? Convert.ToInt32(f_num) : (int?)null,
                                MetaData = mt
                            });
                        }


                        //male
                        var m_den = Convert.ToString(art_sheet.Cells[row + 1, column].Value);
                        var m_num = Convert.ToString(art_sheet.Cells[row + 1, column + 1].Value);

                        if (!string.IsNullOrEmpty(m_den) && !string.IsNullOrEmpty(m_num)) //ignore if both values are empty
                        {
                            art_list.Add(new ART
                            {
                                Site = site,
                                AgeGroup = art_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.M,
                                IndicatorType = indicatorType,
                                Denominator = !string.IsNullOrEmpty(m_den) ? Convert.ToInt32(m_den) : (int?)null,
                                Numerator = !string.IsNullOrEmpty(m_num) ? Convert.ToInt32(m_num) : (int?)null,
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
                        goto Loop; ; // throw new ApplicationException("Invalid facility uploaded");


                    for (int column = 5; column <= 24;)
                    {

                        var less_than_1000 = Convert.ToString(pmtct_viral_load_sheet.Cells[row, column].Value);
                        var _greater_than_1000 = Convert.ToString(pmtct_viral_load_sheet.Cells[row, column + 1].Value);

                        if (!string.IsNullOrEmpty(less_than_1000) && !string.IsNullOrEmpty(_greater_than_1000)) //ignore if both values are empty
                        {
                            _list.Add(new PMTCT_Viral_Load
                            {
                                Site = site,
                                AgeGroup = pmtct_viral_load_sheet.Cells[3, column].Value.ToString(),
                                Category = PMTCT_Category.Newly_Identified,
                                _less_than_1000 = !string.IsNullOrEmpty(less_than_1000) ? Convert.ToInt32(less_than_1000) : (int?)null,
                                _greater_than_1000 = !string.IsNullOrEmpty(_greater_than_1000) ? Convert.ToInt32(_greater_than_1000) : (int?)null,
                                MetaData = mt
                            });
                        }


                        less_than_1000 = Convert.ToString(pmtct_viral_load_sheet.Cells[row + 1, column].Value);
                        _greater_than_1000 = Convert.ToString(pmtct_viral_load_sheet.Cells[row + 1, column + 1].Value);

                        if (!string.IsNullOrEmpty(less_than_1000) && !string.IsNullOrEmpty(_greater_than_1000)) //ignore if both values are empty
                        {
                            _list.Add(new PMTCT_Viral_Load
                            {
                                Site = site,
                                AgeGroup = pmtct_viral_load_sheet.Cells[3, column].Value.ToString(),
                                Category = PMTCT_Category.Already_HIV_Positive,
                                _less_than_1000 = !string.IsNullOrEmpty(less_than_1000) ? Convert.ToInt32(less_than_1000) : (int?)null,
                                _greater_than_1000 = !string.IsNullOrEmpty(_greater_than_1000) ? Convert.ToInt32(_greater_than_1000) : (int?)null,
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

                    if (site == null)
                        goto Loop; ; // throw new ApplicationException("Invalid facility uploaded");

                    for (int column = 5; column <= 24;)
                    {

                        var f_POS = Convert.ToString(linkage_sheet.Cells[row, column].Value);
                        var f_TX_NEW = Convert.ToString(linkage_sheet.Cells[row, column + 1].Value);

                        if (!string.IsNullOrEmpty(f_POS) && !string.IsNullOrEmpty(f_TX_NEW)) //ignore if both values are empty
                        {
                            //female
                            _list.Add(new LinkageToTreatment
                            {
                                Site = site,
                                AgeGroup = linkage_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.F,
                                POS = !string.IsNullOrEmpty(f_POS) ? Convert.ToInt32(f_POS) : (int?)null,
                                Tx_NEW = !string.IsNullOrEmpty(f_TX_NEW) ? Convert.ToInt32(f_TX_NEW) : (int?)null,
                                MetaData = mt
                            });
                        }


                        //male
                        var m_POS = Convert.ToString(linkage_sheet.Cells[row + 1, column].Value); // !string.IsNullOrEmpty() ? Convert.ToInt32(hts_index_sheet.Cells[row + 1, column].Value) : (int?)null;
                        var m_TX_NEW = Convert.ToString(linkage_sheet.Cells[row + 1, column + 1].Value); // !string.IsNullOrEmpty() ? Convert.ToInt32(hts_index_sheet.Cells[row + 1, column + 1].Value) : (int?)null;

                        if (!string.IsNullOrEmpty(m_POS) && !string.IsNullOrEmpty(m_TX_NEW)) //ignore if both values are empty
                        {
                            _list.Add(new LinkageToTreatment
                            {
                                Site = site,
                                AgeGroup = linkage_sheet.Cells[3, column].Value.ToString(),
                                Sex = Sex.M,
                                POS = !string.IsNullOrEmpty(f_POS) ? Convert.ToInt32(m_POS) : (int?)null,
                                Tx_NEW = !string.IsNullOrEmpty(m_TX_NEW) ? Convert.ToInt32(m_TX_NEW) : (int?)null,
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

        public string PopulateTemplate(Profile loggedinProfile)
        {
            var facilities = new MPMDAO().GetPivotTableFromFacility(loggedinProfile.Organization.ShortName);

            string fileName = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/MPM/CDC MPM Tool_" + loggedinProfile.Organization.ShortName + ".xlsm");

            string template = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/MPM/CDC MPM Tool Ver1_01032018.xlsm");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
            {
                var homesheet = package.Workbook.Worksheets["Guide"];
                homesheet.Cells["G14"].Value = loggedinProfile.Organization.ShortName;

                var hts_index_sheet = package.Workbook.Worksheets["HTS_Index"];
                var linkage_sheet = package.Workbook.Worksheets["LInkage to Treatment"];
                var pitc_sheet = package.Workbook.Worksheets["HTS_Other PITC"];
                var pmtct_sheet = package.Workbook.Worksheets["PMTCT"];
                var art_sheet = package.Workbook.Worksheets["ART"];
                var pmtct_viral_load_sheet = package.Workbook.Worksheets["PMTCT_Viral Load"];

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

                package.SaveAs(new FileInfo(fileName));
            }

            return fileName;
        }



    }


}
