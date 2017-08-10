using BWReport.DAL.DAO;
using BWReport.DAL.Entities;
using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BWReport.DAL.Services
{
    public class ReportLoader
    {
        static LGA FindLGA(IList<LGA> lgas, string lgaName, string state = "")
        {
            LGA lga = null;
            string _lga_name = lgaName.ToLower().Replace(" local government area", "").Trim();
            lga = lgas.FirstOrDefault(x => x.lga_name.ToLower() == _lga_name);// && x.State.state_name.ToLower() == _state_name);
            if (lga == null)
            {
                if (_lga_name == "amac")
                {
                    _lga_name = "abuja municipal";
                }
                else if(_lga_name == "ifako")
                {
                    _lga_name = "ifako-ijaye";
                }
                lga = lgas.FirstOrDefault(x => !string.IsNullOrEmpty(x.alternative_name)
                && x.alternative_name.ToLower() == _lga_name);// && x.State.state_name.ToLower() == _state_name);
            }
            return lga;
        }


        public bool ExtractReport(string reportingPeriod, int Year, int startColumnIndex, Stream ReportStream, string loggedinUser, string fileName)
        {
            List<PerformanceData> ActualPerformanceMeasures = new List<PerformanceData>();

            PerformanceDataDao _targetDao = new PerformanceDataDao();
            bwrHealthFacilityDAO sdfDao = new bwrHealthFacilityDAO();
            var LGADictionary = new LGADao().RetrieveAll();
            var existingFacilities = sdfDao.RetrieveAll();
            
            try
            {
                using (ExcelPackage package = new ExcelPackage(ReportStream))//(new FileInfo(FilePath)))
                {
                    var dashbardSheet = package.Workbook.Worksheets["Dashboard Navigation"];
                    string ImplementingPartner = (string)dashbardSheet.Cells["B9"].Value;
                    Organizations ip = new OrganizationDAO().SearchByShortName(ImplementingPartner);
                    if (ip == null)
                    {
                        throw new ApplicationException("Unknown IP");
                    }

                    ReportUploads upload = null;
                    //search previous report for duplicate using date
                    var previousUploads = new ReportUploadsDao().SearchPreviousUpload(reportingPeriod, Year, ImplementingPartner, fileName);
                    if (previousUploads != null && previousUploads.Count() > 0)
                    {
                        throw new ApplicationException("report already uploaded for the selected period");
                    }
                    if (previousUploads == null || previousUploads.Count == 0)
                    {
                        upload = new ReportUploads
                        {
                            ReportName = fileName,
                            DateUploaded = DateTime.Now,
                            ImplementingPartner = ImplementingPartner,
                            UploadingUser = loggedinUser,
                            ReportingPeriod = reportingPeriod,
                            FY = Year,
                        };
                        new ReportUploadsDao().Save(upload);
                    }

                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string name = sheet.Name;
                        if (name.ToLower().Contains("dashboard") || name.Contains("LGA"))
                            continue;

                        string sheetTitle = ExcelHelper.ReadCellText(sheet, 1, 1);

                        LGA theLGA = FindLGA(LGADictionary, sheetTitle);

                        //LGADictionary.TryGetValue(sheetTitle, out theLGA);
                        if (theLGA == null)
                        {
                            throw new ApplicationException("invalid LGA on the sheet - " + name);
                        }
                        int row = 8;
                       

                        while (true)
                        {
                            string facilityName = ExcelHelper.ReadCellText(sheet, row, 2);
                            if (string.IsNullOrEmpty(facilityName))
                            {
                                break;
                            }

                            string facilityType = ExcelHelper.ReadCellText(sheet, row, 3);
                            string fType = !string.IsNullOrEmpty(facilityType) ? facilityType.Substring(0, 1) : "F";
                            //string facilityCode = ExcelHelper.ReadCellText(sheet, row, 1); //GetFacilityCode(sheet, row, ImplementingPartner, theLGA, fType);
                            CommonUtil.Enums.OrganizationType orgType = facilityType.StartsWith("F") ? CommonUtil.Enums.OrganizationType.HealthFacilty : CommonUtil.Enums.OrganizationType.CommunityBasedOrganization;

                           // if (!string.IsNullOrEmpty(facilityCode))
                            {
                                //int fCount = existingFacilities.Count(f => f.FacilityName == facilityName && f.LGA == theLGA && f.Organization == ip);
                                //if (fCount > 1)
                                //{
                                //    throw new ApplicationException(string.Format("Conflict in Facility name, {0} in sheet {1}", facilityName, theLGA.lga_name));
                                //}

                                bwrHealthFacility theFacility = existingFacilities.FirstOrDefault(f => f.FacilityName == facilityName && f.LGA == theLGA && f.Organization == ip);

                                if (theFacility == null)
                                {
                                    theFacility = new bwrHealthFacility
                                    {
                                        Organization = ip,
                                        LGA = theLGA,
                                        FacilityName = facilityName,
                                        OrganizationType = orgType
                                    };
                                    sdfDao.Save(theFacility);
                                }
                                //correct the the facility type here
                                //the list from datim did not specify faciity type
                                else if (orgType != theFacility.OrganizationType)
                                {
                                    theFacility.OrganizationType = orgType;
                                    sdfDao.Update(theFacility);
                                }

                                if (theFacility.LGA != theLGA)
                                {
                                    throw new ApplicationException(string.Format("{0} Wrongly included in {1} sheet", facilityName, theLGA.lga_name));
                                }

                                //incase of repetition
                                if (ActualPerformanceMeasures.Any(a => a.HealthFacility == theFacility) == false)
                                {
                                    int columnStart = startColumnIndex; //12
                                    int HTC_TST = 0;
                                    int HTC_TST_pos = 0;
                                    int Tx_NEW = 0;
                                    int.TryParse(ExcelHelper.ReadCellText(sheet, row, columnStart), out HTC_TST);
                                    int.TryParse(ExcelHelper.ReadCellText(sheet, row, columnStart + 1), out HTC_TST_pos);
                                    int.TryParse(ExcelHelper.ReadCellText(sheet, row, columnStart + 2), out Tx_NEW);

                                    var performanceMeasure = new PerformanceData
                                    {
                                        HTC_TST = HTC_TST,
                                        HTC_TST_POS = HTC_TST_pos,
                                        Tx_NEW = Tx_NEW,
                                        HealthFacility = theFacility,
                                        ReportPeriod = reportingPeriod,
                                        FY = Year,
                                        ReportUpload = upload,
                                    };
                                    ActualPerformanceMeasures.Add(performanceMeasure);
                                }
                            }
                            //else
                            //{
                            //    throw new ApplicationException("Facility code is empty for site -" + facilityName);
                            //}
                            row++;
                        }
                    }
                }

                if (ActualPerformanceMeasures == null || ActualPerformanceMeasures.Count() == 0)
                {
                    throw new ApplicationException("No Valid facility found. Please cross-check the template and ensure that DATIM facility codes are included");
                }

                //save here
                bool saved = false;

                saved = _targetDao.BulkInsert(ActualPerformanceMeasures);
                _targetDao.CommitChanges();
                return saved;

            }
            catch (Exception ex)
            {
                _targetDao.RollbackChanges();
                throw ex;
            }
        }

        private string GetFacilityCode(ExcelWorksheet sheet, int row, string IP, LGA lga, string FacilityType)
        {

            string facilityCode = ExcelHelper.ReadCell(sheet, row, 1);
            if (string.IsNullOrEmpty(facilityCode))
            {
                string[] lgaArr = lga.lga_code.Split(' ');
                facilityCode = string.Format("{0}/{1}/{2}/{3}/{4}", IP, lgaArr[1], lgaArr[2], FacilityType, (row - 7).ToString().PadLeft(4, '0'));
            }

            return facilityCode;
        }


        public void GenerateExcel(string newTemplate, string existingTemplate, string IP, int year)
        {
            PerformanceDataDao _targetDao = new PerformanceDataDao();
            var IndexPeriods = ExcelHelper.GenerateIndexedPeriods();
            var LGADictionary = new LGADao().RetrieveAll().ToDictionary(x => x.lga_code);

            var pds = _targetDao.RetrieveByOrganizationShortName(year, IP)
                .OrderBy(x => x.HealthFacility.FacilityName).GroupBy(x => x.HealthFacility.LGA);

            using (ExcelPackage package = new ExcelPackage(new FileInfo(existingTemplate)))
            {
                foreach (var item in pds)
                {
                    string lgaName = item.Key.lga_name;

                    ExcelWorksheet sheet = RetrieveMatchingWorkSheet(lgaName, package.Workbook.Worksheets, LGADictionary);
                    if (sheet == null)
                        continue;

                    int row = 8;
                    int columnStart = 0;

                    var groupedByFacility = item.ToList().GroupBy(x => x.HealthFacility);

                    foreach (var f in groupedByFacility)
                    {


                        sheet.Cells[row, columnStart + 2].Value = f.Key.FacilityName;

                        switch (f.Key.OrganizationType)
                        {
                            case CommonUtil.Enums.OrganizationType.CommunityBasedOrganization:
                                sheet.Cells[row, columnStart + 3].Value = "Community";
                                break;
                            case CommonUtil.Enums.OrganizationType.HealthFacilty:
                                sheet.Cells[row, columnStart + 3].Value = "Facility";
                                break;
                            default:
                                sheet.Cells[row, columnStart + 3].Value = "Unknown";
                                break;
                        }

                        var lineEntries = f.OrderBy(x => x.HealthFacility.FacilityName).ToList();
                        for (int i = 0; i < lineEntries.Count; i++)
                        {
                            var aLine = lineEntries[i];
                            int valueStartingPoint = IndexPeriods[aLine.ReportPeriod];

                            sheet.Cells[row, valueStartingPoint].Value = aLine.HTC_TST;
                            sheet.Cells[row, valueStartingPoint + 1].Value = aLine.HTC_TST_POS;
                            sheet.Cells[row, valueStartingPoint + 2].Value = aLine.Tx_NEW;

                        }

                        row += 1;

                    }



                }
                package.SaveAs(new FileInfo(newTemplate));
            }
        }

        private ExcelWorksheet RetrieveMatchingWorkSheet(string lgaName, ExcelWorksheets worksheets, Dictionary<string, LGA> lGADictionary)
        {
            ExcelWorksheet sheet = null;
            foreach (var sh in worksheets)
            {
                string sheet_lga = ExcelHelper.ReadCell(sh, 1, 1);

                LGA theLGA = null;
                lGADictionary.TryGetValue("NIE " + sheet_lga, out theLGA);
                if (theLGA == null)
                {
                    continue;
                }
                if (theLGA.lga_name == lgaName)
                    sheet = sh;
            }

            return sheet;
        }
    }

    public enum MonthAbreviation
    {
        Jan = 1,
        Feb = 2,
        Mar = 3,
        Apr = 4,
        May = 5,
        Jun = 6,
        Jul = 7,
        Aug = 8,
        Sep = 9,
        Oct = 10,
        Nov = 11,
        Dec = 12
    }
}

