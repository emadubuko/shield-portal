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

        public bool ExtractReport(DateTime reportingPeriodFrom, DateTime reportingPeriodTo, string FilePath, string fileName, Stream ReportStream, string loggedinUser)
        {
            List<HealthFacility> Facilities = new List<HealthFacility>();
            List<PerformanceData> TargetMeasures = new List<PerformanceData>();

            PerformanceDataDao _targetDao = new PerformanceDataDao();
            SDFDao sdfDao = new SDFDao();
            IList<LGA> LGAs = new LGADao().RetrieveAll();

            var upload = new ReportUploads
            {
                DateUploaded = DateTime.Now,
                ReportName = fileName,
                 ImplementingPartner = null,
                UploadingUser =  loggedinUser,
                FileLocation = FilePath,
                ReportingPeriodFrom = reportingPeriodFrom,
                ReportingPeriodTo = reportingPeriodTo
            };
            new ReportUploadsDao().Save(upload);

            using (var fileStream = File.Create(FilePath))
            {
                ReportStream.Seek(0, SeekOrigin.Begin);
                ReportStream.CopyTo(fileStream);
            }
            try
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(FilePath)))
                {
                    foreach (var sheet in package.Workbook.Worksheets)
                    {
                        string name = sheet.Name;

                        string sheetTitle = ReadCell(sheet, 1, 1);
                        if (sheetTitle.ToLower().Trim() != name.ToLower().Trim())
                        {
                            continue;
                        }

                        if (sheetTitle == "AMAC")
                        {
                            sheetTitle = "Municipal Area Council";
                        }
                        LGA theLGA = LGAs.FirstOrDefault(x => x.Name.ToLower().Trim() == sheetTitle.ToLower().Trim()); //LGAs[sheetTitle]; //search the dictionary
                        if (theLGA == null)
                        {
                            continue;
                        }
                        int row = 8;

                        while (true)
                        {
                            int columnStart = 11;
                            string facilityName = ReadCell(sheet, row, 2);
                            if (string.IsNullOrEmpty(facilityName))
                            {
                                break;
                            }

                            HealthFacility sdf = new HealthFacility
                            {
                                Name = facilityName,
                                LGA = theLGA,
                                State = theLGA.State,
                                //HTC_TST_Target = ReadCell(sheet, row, 4),
                                //HTC_TST_POS_Target = ReadCell(sheet, row, 6),
                                //Tx_NEW_Target = ReadCell(sheet, row, 8),
                                Latitude = "",
                                Longitude = "",
                                PerfomanceData = new List<PerformanceData>()
                            };

                            Facilities.Add(sdf);

                            bool stopColumn_loop = true;
                            while (stopColumn_loop)
                            {
                                string[] period = ReadCell(sheet, 1, columnStart).Split(new string[] { "-", " " }, StringSplitOptions.RemoveEmptyEntries);
                                string HTC_TST = ReadCell(sheet, row, columnStart);
                                string HTC_TST_pos = ReadCell(sheet, row, columnStart + 1);
                                string Tx_NEW = ReadCell(sheet, row, columnStart + 2);

                                if (period.Count() < 3 || period[0] == "")
                                {
                                    stopColumn_loop = false;
                                }
                                else
                                {
                                    var newTarget = new PerformanceData
                                    {
                                        HTC_TST = HTC_TST,
                                        HTC_TST_POS = HTC_TST_pos,
                                        Tx_NEW = Tx_NEW,
                                        HealthFacility = sdf,
                                        ReportPeriodFrom = new DateTime(2016, (int)Enum.Parse(typeof(MonthAbreviation), period[2]), Convert.ToInt32(period[0])),
                                        ReportPeriodTo = new DateTime(2016, (int)Enum.Parse(typeof(MonthAbreviation), period[2]), Convert.ToInt32(period[1])),
                                        ReportUpload = upload,
                                    };
                                    TargetMeasures.Add(newTarget);
                                    sdf.PerfomanceData.Add(newTarget);

                                    columnStart = columnStart + 3;
                                }
                            }
                            row++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //save here
            bool saved = false;
            try
            {
                sdfDao.BulkInsert(Facilities);
                saved = _targetDao.BulkInsert(TargetMeasures);
            }
            catch
            {
                _targetDao.RollbackChanges();
            }

            if (saved)
                _targetDao.CommitChanges();

            return saved;
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

