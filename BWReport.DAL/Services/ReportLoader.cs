using BWReport.DAL.DAO;
using BWReport.DAL.Entities;
using CommonUtil.DAO;
using CommonUtil.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BWReport.DAL.Services
{
    public class ReportLoader
    {
        private string ReadCell(ExcelWorksheet sheet, int row, int column)
        {
            var range = sheet.Cells[row, column] as ExcelRange;
            if (range.Value != null)
            {
                return range.Value.ToString();
            }
            return "";
        }

        public bool ExtractReport(string reportingPeriod, int Year, int startColumnIndex, string ImplementingPartner,  Stream ReportStream, string loggedinUser)
        {
            List<PerformanceData> ActualPerformanceMeasures = new List<PerformanceData>();

            PerformanceDataDao _targetDao = new PerformanceDataDao();
            HealthFacilityDAO sdfDao = new HealthFacilityDAO();
            var LGADictionary = new LGADao().RetrieveAll().ToDictionary(x => x.lga_code);
            var existingFacilities = sdfDao.RetrieveAll().ToDictionary(x => x.FacilityCode);

           
            try
            {
                using (ExcelPackage package = new ExcelPackage(ReportStream))//(new FileInfo(FilePath)))
                {
                    //search previous report for duplicate using date
                    var previousUploads = new ReportUploadsDao().SearchPreviousUpload(reportingPeriod, Year, ImplementingPartner);
                    if (previousUploads != null && previousUploads.Count() > 0)
                    {
                        throw new ApplicationException("report already uploaded for the selected period");
                    }

                    var upload = new ReportUploads
                    {
                        DateUploaded = DateTime.Now, 
                        ImplementingPartner = ImplementingPartner,
                        UploadingUser = loggedinUser,
                        ReportingPeriod = reportingPeriod,
                        FY = Year,
                    };
                    new ReportUploadsDao().Save(upload);

                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string name = sheet.Name;
                        if (name.ToLower().Contains("dashboard"))
                            continue;

                        string sheetTitle = ReadCell(sheet, 1, 1);

                        LGA theLGA = null;
                        LGADictionary.TryGetValue("NIE " + sheetTitle, out theLGA);
                        if (theLGA == null)
                        {
                            continue;
                        }
                        int row = 8;

                        while (true)
                        {
                            string facilityName = ReadCell(sheet, row, 3);
                            if (string.IsNullOrEmpty(facilityName))
                            {
                                break;
                            }

                            string facilityType = ReadCell(sheet, row, 4);
                            string facilityCode = GetFacilityCode(sheet, row, ImplementingPartner, theLGA, facilityType.Substring(0, 1));

                            HealthFacility theFacility = null;
                            existingFacilities.TryGetValue(facilityCode, out theFacility);

                            if (theFacility == null)
                            {
                                theFacility = new HealthFacility
                                {
                                    FacilityCode = facilityCode,
                                    LGA = theLGA,
                                    Name = facilityName,
                                    OrganizationType = facilityType.StartsWith("F") ? CommonUtil.Enums.OrganizationType.HealthFacilty : CommonUtil.Enums.OrganizationType.CommunityBasedOrganization,
                                };
                                sdfDao.Save(theFacility);
                            }

                            int columnStart = startColumnIndex; //12
                            int HTC_TST = 0;
                            int HTC_TST_pos = 0;
                            int Tx_NEW = 0;
                            int.TryParse(ReadCell(sheet, row, columnStart), out HTC_TST);
                            int.TryParse(ReadCell(sheet, row, columnStart + 1), out HTC_TST_pos);
                            int.TryParse(ReadCell(sheet, row, columnStart + 2), out Tx_NEW);

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
                            row++;
                        }
                    }
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

        private string GetFacilityCode(ExcelWorksheet sheet, int row, string IP, LGA lga, string FacilityType )
        {

            string facilityCode = ReadCell(sheet, row, 1);
            if (string.IsNullOrEmpty(facilityCode))
            {
                string[] lgaArr = lga.lga_code.Split(' ');
                facilityCode = string.Format("{0}/{1}/{2}/{3}/{4}", IP, lgaArr[1], lgaArr[2], FacilityType, (row - 7).ToString().PadLeft(4, '0'));
            }

            return facilityCode;
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

