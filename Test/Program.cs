using BWReport.DAL.DAO;
using BWReport.DAL.Entities;
using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DAL.DAO;
using NHibernate;
using NHibernate.Engine;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Threading;
using DAL.Entities;
using System.Collections.Concurrent;
using RADET.DAL.Entities;
using OfficeOpenXml.DataValidation;
using System.Globalization;
using System.Xml.Linq;

namespace Test
{
    public class St
    {
        public string State { get; set; }
        public List<string> LGAList { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LGADao dao = new LGADao();
            var lga = dao.RetrieveAll().GroupBy(x=>x.State.state_name);
            var states = new List<St>();
            foreach(var g in lga)
            {
                states.Add(new St
                {
                    State = g.Key,
                    LGAList = g.ToList().Select(x=>x.lga_name).ToList()
                });
            }
            
            string j = Newtonsoft.Json.JsonConvert.SerializeObject(states);
            Console.WriteLine(j);

            Console.WriteLine("started");

            TransfromColumns();

            //ReadExcelFiles();

            //ReadIndicatorValues();

            //CopyExcelFile();
            // ReadExcelFiles();
            // GetARTSite();
            //exportRadetErrors();

            // RadetExcelDocs.ReturnLGA_n_State(@"C:\MGIC\Radet extracted\Live Radet\All IPs Unzipped\Q2_RADET_ECEWS\Q2_RADET_ECEWS_ GH Okigwe.xlsx"); //ReadAndMerge();

            //  MosisFiles.ProcessHTS_TST_File();

            //UpdateFacilities();

            // AfenetUtil.ReadFile();
            //

            //DMPSiteType();
            //new ShufflingTest().TestShuffle();

            // new Program().RetrieveDimensionValue();

            // new Program().RetrieveExcelValue();


            //  DMPSummary();

            //new Program().UpdateFacilities();
            //    new Program().GenerateFacilityTargetCSV();

            // new Program().GenerateNonDatimCode();

            //new Program().GenerateFacilityCode();
            //new Program().UnprotectExcelFile();
            //  new Program().CopyAndPaste();

            Console.ReadLine();
        }

        static void ReadIndicatorValues()
        {
            StringBuilder sb = new StringBuilder();

            string baseLocation = @"C:\Users\DevUser2\Desktop\DQA Q4 FY17 before pvls correction\DQA Q4 FY17\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);

            foreach (string filename in files)
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filename)))
                {
                    try
                    {
                        var summaryworksheet = package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"];
                        var worksheet = package.Workbook.Worksheets["Worksheet"];

                        int i = 6;
                        var summaries = new XElement("summaries");
                        var reported_data = new XElement("reported_data");
                        reported_data.Add(new XElement("HTC_TST", summaryworksheet.Cells[i, 2].Value.ToString()));
                        reported_data.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
                        reported_data.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
                        reported_data.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));
                        reported_data.Add(new XElement("PMTCT_STAT", summaryworksheet.Cells[i, 6].Value.ToString()));
                        reported_data.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
                        reported_data.Add(new XElement("PMTCT_STAT_Knwpos", summaryworksheet.Cells[i, 8].Value.ToString()));
                        reported_data.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
                        reported_data.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
                        reported_data.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
                        reported_data.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
                        reported_data.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
                        reported_data.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
                        reported_data.Add(new XElement("PMTCT_FO", summaryworksheet.Cells[i, 15].Value.ToString()));
                        reported_data.Add(new XElement("TX_Curr", summaryworksheet.Cells["E12"].Value.ToString()));
                        reported_data.Add(new XElement("TX_RET", summaryworksheet.Cells["J12"].Value.ToString()));
                        reported_data.Add(new XElement("TX_PLVS", worksheet.Cells["X10"].Value.ToString()));
                        summaries.Add(reported_data);

                        i = 7;
                        var validation = new XElement("validation");
                        validation.Add(new XElement("HTC_TST", summaryworksheet.Cells[i, 2].Value.ToString()));
                        validation.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
                        validation.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
                        validation.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));
                        validation.Add(new XElement("PMTCT_STAT", summaryworksheet.Cells[i, 6].Value.ToString()));
                        validation.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
                        validation.Add(new XElement("PMTCT_STAT_Knwpos", summaryworksheet.Cells[i, 8].Value.ToString()));
                        validation.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
                        validation.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
                        validation.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
                        validation.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
                        validation.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
                        validation.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
                        validation.Add(new XElement("PMTCT_FO", summaryworksheet.Cells[i, 15].Value.ToString()));
                        validation.Add(new XElement("TX_Curr", summaryworksheet.Cells["E13"].Value.ToString()));
                        validation.Add(new XElement("TX_RET", summaryworksheet.Cells["J13"].Value.ToString()));
                        validation.Add(new XElement("TX_PLVS", worksheet.Cells["X11"].Value.ToString()));
                        summaries.Add(validation);

                        i = 8;
                        var concurrency_rate = new XElement("Concurrence_rate");
                        concurrency_rate.Add(new XElement("HTC_TST", summaryworksheet.Cells[i, 2].Value.ToString()));
                        concurrency_rate.Add(new XElement("HTC_TST_Pos", summaryworksheet.Cells[i, 3].Value.ToString()));
                        concurrency_rate.Add(new XElement("HTC_Only", summaryworksheet.Cells[i, 4].Value.ToString()));
                        concurrency_rate.Add(new XElement("HTC_Pos", summaryworksheet.Cells[i, 5].Value.ToString()));
                        concurrency_rate.Add(new XElement("PMTCT_STAT", summaryworksheet.Cells[i, 6].Value.ToString()));
                        concurrency_rate.Add(new XElement("PMTCT_STAT_Pos", summaryworksheet.Cells[i, 7].Value.ToString()));
                        concurrency_rate.Add(new XElement("PMTCT_STAT_Knwpos", summaryworksheet.Cells[i, 8].Value.ToString()));
                        concurrency_rate.Add(new XElement("PMTCT_ART", summaryworksheet.Cells[i, 9].Value.ToString()));
                        concurrency_rate.Add(new XElement("PMTCT_EID", summaryworksheet.Cells[i, 10].Value.ToString()));
                        concurrency_rate.Add(new XElement("TX_NEW", summaryworksheet.Cells[i, 11].Value.ToString()));
                        concurrency_rate.Add(new XElement("TB_STAT", summaryworksheet.Cells[i, 12].Value.ToString()));
                        concurrency_rate.Add(new XElement("TB_ART", summaryworksheet.Cells[i, 13].Value.ToString()));
                        concurrency_rate.Add(new XElement("TX_TB", summaryworksheet.Cells[i, 14].Value.ToString()));
                        concurrency_rate.Add(new XElement("PMTCT_FO", summaryworksheet.Cells[i, 15].Value.ToString()));
                        concurrency_rate.Add(new XElement("TX_Curr", summaryworksheet.Cells["E14"].Value.ToString()));
                        concurrency_rate.Add(new XElement("TX_RET", summaryworksheet.Cells["J14"].Value.ToString()));
                        concurrency_rate.Add(new XElement("TX_PLVS", worksheet.Cells["X12"].Value.ToString()));
                        summaries.Add(concurrency_rate);

                        var facilityname = worksheet.Cells["V2"].Text;
                        var facilitycode = worksheet.Cells["AA2"].Text;

                        sb.AppendLine(string.Format("{0},{1},{2}", summaries.ToString(), facilitycode, facilityname));

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        continue;
                    }
                }
            }
            File.WriteAllText("C:/Users/DevUser2/Desktop/csv.csv", sb.ToString());
        }

        public static DateTime? ConvertQuarterToEndDate(string RadetPeriod)
        {
            string quarter = RadetPeriod.Split(' ')[0];
            string year = RadetPeriod.Substring(5, 2);
            string endDay = "";
            switch (quarter)
            {
                case "Q1": endDay = "31/12/" + year; break;
                case "Q2": endDay = "31/3/" + year; break;
                case "Q3": endDay = "30/6/" + year; break;
                case "Q4": endDay = "30/9/" + year; break;
            }
            if (!string.IsNullOrEmpty(endDay))
            {

            }
            DateTime date;
            DateTime.TryParseExact(endDay, "d/m/yy", new CultureInfo("en-US"), DateTimeStyles.None, out date);
            return date;
        }

        public static Dictionary<string, List<string>> GetARTSite()
        {
            Dictionary<string, List<string>> artSites = new Dictionary<string, List<string>>();
            using (var package = new ExcelPackage(new FileInfo(@"C:\MGIC\radet docs\RADET Supplementary v3.04e.xlsx"))) // (@"C:\Users\cmadubuko\Google Drive\MGIC\Project\ShieldPortal\DMP\Report\Template\ART sites.xlsx")))
            {
                var aSheet = package.Workbook.Worksheets["JustRADETNames"];

                for (int col = 2; col <= 776; col++)
                {
                    string lga = ExcelHelper.ReadCellText(aSheet, 1, col);
                    if (string.IsNullOrEmpty(lga))
                        break;

                    List<string> facilities = new List<string>();
                    int row = 2;
                    while (true)
                    {
                        var text = aSheet.Cells[row, col];
                        string facility = text.Text != null ? text.Text : ""; //ExcelHelper.ReadCellText(aSheet, row, 4);
                        if (string.IsNullOrEmpty(facility))
                            break;
                        facilities.Add(facility);
                        row++;
                    }
                    artSites.Add(lga, facilities);
                }
            }
            return artSites;
        }

        private static void ReadExcelFiles()
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(@"C:\MGIC\Document\RADET\RADET v3.06b FY18 Edition.xlsx")))
            {
                var sheet = package.Workbook.Worksheets["StateLGA"];
                var stateCells = sheet.Cells;
                using (var excelToExport = new ExcelPackage(new FileInfo(@"C:\MGIC\Document\RADET\StateLGAQ4.xlsx")))
                {
                    var aSheet = excelToExport.Workbook.Worksheets["Sheet1"];
                    foreach (var cls in stateCells)
                    {
                        aSheet.Cells[cls.Address].Value = cls.Value;
                    }

                    excelToExport.Save();
                }

                //var mainsheet = package.Workbook.Worksheets["MainPage"];
                //var mainPageCells = mainsheet.Cells;
                //var validations = mainsheet.DataValidations;

                //foreach (var validation in validations)
                //{
                //    var list = validation as ExcelDataValidationList;
                //    if (list != null)
                //    {                        
                //        var rowStart = list.Address.Start.Row;
                //        var rowEnd = list.Address.End.Row;
                //        // allowed values probably only in one column....
                //        var colStart = list.Address.Start.Column;
                //        var colEnd = list.Address.End.Column;

                //        for (int row = rowStart; row <= rowEnd; ++row)
                //        {
                //            for (int col = colStart; col <= colEnd; col++)
                //            {
                //                Console.WriteLine(mainsheet.Cells[row, col].Value);
                //            }
                //        }
                //    }
                //}


                //package.SaveAs(new FileInfo(@"C:\MGIC\DQA\STATELGA.xlsx"));
            }
        }

        private static void TransfromColumns()
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(@"C:\MGIC\Document\RADET\StateLGAQ4.xlsx")))
            {
                var sheet = package.Workbook.Worksheets["Sheet2"];
                var facSheet = package.Workbook.Worksheets["facility transform"];

                var stateCells = sheet.Cells;
                int oRow = 1;

                for (int col = 1; col <= 774; col++)
                {
                    var lga = sheet.Cells[1, col].Value;

                    for (int row = 3; ; ++row)
                    {
                        var cell = sheet.Cells[row, col];

                        if (cell == null || cell.Value == null)
                            break;

                        facSheet.Cells[oRow, 1].Value = cell.Value;
                        facSheet.Cells[oRow, 2].Value = lga;

                        oRow += 1; 
                    } 
                }
                package.Save();
            }            
        }
 

        private static void CopyExcelFile()
        {

            using (ExcelPackage package = new ExcelPackage(new FileInfo(@"C:\MGIC\Tina\TAU Data Analysis.16Oct17.xlsx")))
            {
                var sheets = package.Workbook.Worksheets;
                foreach (var sheet in sheets)
                {
                    int row = 4;
                    List<string> faclities = new List<string>();
                    while (true)
                    {
                        var text = sheet.Cells["B" + row];
                        if (text == null || string.IsNullOrEmpty(text.Text))
                            break;
                        faclities.Add(text.Text);
                        row++;
                    }
                    faclities.Shuffle();
                    using (var excelToExport = new ExcelPackage(new FileInfo(@"C:\MGIC\Tina\TAU Data Analysis.16Oct17.Randomized.xlsx")))
                    {
                        var aSheet = excelToExport.Workbook.Worksheets[sheet.Name];
                        int i = 4;
                        foreach (var cls in faclities.Take(10))
                        {
                            aSheet.Cells["B" + i].Value = cls;
                            i++;
                        }
                        excelToExport.Save();
                    }
                }
            }
        }




        static void exportRadetErrors()
        {
            var errors = (from item in new RADET.DAL.DAO.RadetUploadErrorLogDAO().Search()//.RetrieveAll();
                          where item.RadetUpload.IP.ShortName == "IHVN"
                          select item).ToList();

            foreach (var err in errors)//.GroupBy(x => x.RadetUpload.IP))
            {
                string fileName = "_ihvn.err"; // err.Key.ShortName + ".csv"; //string.Format("{0:dd-MM-yyyy hh.mm.ss. tt}.csv", DateTime.Now);
                StringBuilder sb = new StringBuilder();
                sb.Append("File Name,");
                sb.Append("Tab Name,");
                sb.Append("Error Message,");
                sb.Append("Line No,");
                sb.Append("Patient Id,");

                sb.AppendLine();

                UploadLogError model = err;//.LastOrDefault(x => x.ErrorDetails != null && x.ErrorDetails.Count > 0);

                if (model == null) continue;
                foreach (var m in model.ErrorDetails)
                {
                    sb.Append(string.Format("\"{0}\",", m.FileName));
                    sb.Append(string.Format("\"{0}\",", m.FileTab));
                    sb.Append(string.Format("\"{0}\",", m.ErrorMessage.Replace("<span style='color:red'>", "").Replace("</span>", "")));
                    sb.Append(string.Format("\"{0}\",", m.LineNo));
                    sb.Append(string.Format("\"{0}\",", m.PatientNo));
                    sb.AppendLine();
                }

                File.AppendAllText(@"C:\MGIC\Radet extracted\RADET 2\errors\" + fileName, sb.ToString());
            }
            Console.WriteLine("done writting errors to file");
        }

        private static void UpdateFacilities()
        {
            HealthFacilityDAO dao = new HealthFacilityDAO();
            var hfs = dao.RetrieveAll().ToDictionary(x => x.FacilityCode);
            List<HealthFacility> newHfs = new List<HealthFacility>();

            var file = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\HealthFacility.xlsx";

            using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
            {
                int row = 2;
                var sheet = package.Workbook.Worksheets.FirstOrDefault();
                while (row < 1505)
                {
                    string code = sheet.Cells["D" + row].Value.ToString();
                    HealthFacility hf = null;
                    bool exist = hfs.TryGetValue(code, out hf);
                    LGA lga = RetrieveLga(sheet.Cells["A" + row].Value.ToString(), sheet.Cells["B" + row].Value.ToString().Substring(3));
                    Organizations org = new OrganizationDAO().SearchByShortName(sheet.Cells["E" + row].Value.ToString().Trim());
                    if (exist)
                    {
                        hf.Name = sheet.Cells["C" + row].Value.ToString();
                        if (lga != null)
                        {
                            hf.LGA = lga;
                        }
                        hf.Organization = org;
                        dao.Update(hf);
                    }
                    else
                    {
                        newHfs.Add(new HealthFacility
                        {
                            FacilityCode = code,
                            LGA = lga,
                            Organization = org,
                            OrganizationType = CommonUtil.Enums.OrganizationType.HealthFacilty,
                            Name = sheet.Cells["C" + row].Value.ToString(),
                            lgacode = lga.lga_code,
                        });
                    }
                    row++;
                }
                dao.BulkInsert(newHfs);
                dao.CommitChanges();
            }

        }

        public static LGA RetrieveLga(string statename, string lga)
        {
            LGA l = null;
            try
            {
                LGADao dao = new LGADao();
                l = dao.RetrievebyLGA_State(lga, statename);

            }
            catch { }
            return l;
        }

        private void RetrieveDimensionValue()
        {
            string baseLocation = @"C:\Users\cmadubuko\Desktop\DQA 22nd march\Downloads\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("FacilityName, FacilityCode, HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts, Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID, PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW, TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT, Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr, HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions, Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID, PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions, Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR, Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID, Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr, Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID, Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr");

            Excel.Application xApp = new Excel.Application();
            var oldAlert = xApp.DisplayAlerts;
            xApp.DisplayAlerts = false;
            foreach (var file in files)
            {
                string FacilityName = "", FacilityCode = "";
                var wkb = xApp.Workbooks.Open(file, 0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true); //(file);
                Excel.Worksheet sht = wkb.Sheets["Worksheet"];
                FacilityName = RetrieveExcelValueUsingInterop(sht, 2, 22);
                FacilityCode = RetrieveExcelValueUsingInterop(sht, 2, 27);

                sht = wkb.Sheets["All Questions"];

                string HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts,
                    Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID,
                    PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW,
                    TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT,
                    Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr,
                    HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions,
                    Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID,
                    PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions,
                    Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR,
                    Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID,
                    Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr,
                    Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID,
                    Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr;

                HTC_Charts = RetrieveExcelValueUsingInterop(sht, 6, 6);
                PMTCT_STAT_charts = RetrieveExcelValueUsingInterop(sht, 49, 6);
                PMTCT_EID_charts = RetrieveExcelValueUsingInterop(sht, 117, 6);
                PMTCT_ARV_Charts = RetrieveExcelValueUsingInterop(sht, 91, 6);
                TX_NEW_charts = RetrieveExcelValueUsingInterop(sht, 142, 6);
                TX_CURR_charts = RetrieveExcelValueUsingInterop(sht, 167, 6);

                HTC_Charts_Precisions = RetrieveExcelValueUsingInterop(sht, 2, 6);
                PMTCT_STAT_Charts_Precisions = RetrieveExcelValueUsingInterop(sht, 46, 6);
                PMTCT_EID_Charts_Precisions = RetrieveExcelValueUsingInterop(sht, 117, 6);
                PMTCT_ARV_Charts_Precisions = RetrieveExcelValueUsingInterop(sht, 91, 6);
                TX_NEW_Charts_Precisions = RetrieveExcelValueUsingInterop(sht, 141, 6);
                TX_CURR_Charts_Precisions = RetrieveExcelValueUsingInterop(sht, 166, 6);


                sht = wkb.Sheets["DQA Summary (Map to Quest Ans)"];

                Total_Completeness_HTC_TST = RetrieveExcelValueUsingInterop(sht, 8, 23);
                Total_Completeness_PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 12, 23);
                Total_completeness_PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 15, 23);
                Total_completeness_PMTCT_ARV = RetrieveExcelValueUsingInterop(sht, 16, 23);
                Total_completeness_TX_NEW = RetrieveExcelValueUsingInterop(sht, 17, 23);
                Total_completeness_TX_CURR = RetrieveExcelValueUsingInterop(sht, 18, 23);

                Total_consistency_HTC_TST = RetrieveExcelValueUsingInterop(sht, 8, 24);
                Total_consistency_PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 12, 24);
                Total_consistency_PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 15, 24);
                Total_consistency_PMTCT_ART = RetrieveExcelValueUsingInterop(sht, 16, 24);
                Total_consistency_TX_NEW = RetrieveExcelValueUsingInterop(sht, 17, 24);
                Total_consistency_TX_Curr = RetrieveExcelValueUsingInterop(sht, 18, 24);

                Total_precision_HTC_TST = RetrieveExcelValueUsingInterop(sht, 8, 25);
                Total_precision_PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 12, 25);
                Total_precision_PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 15, 25);
                Total_precision_PMTCT_ARV = RetrieveExcelValueUsingInterop(sht, 16, 25);
                Total_precision_TX_NEW = RetrieveExcelValueUsingInterop(sht, 17, 25);
                Total_precision_TX_CURR = RetrieveExcelValueUsingInterop(sht, 18, 25);

                Total_integrity_HTC_TST = RetrieveExcelValueUsingInterop(sht, 8, 26);
                Total_integrity_PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 12, 26);
                Total_integrity_PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 15, 26);
                Total_integrity_PMTCT_ART = RetrieveExcelValueUsingInterop(sht, 16, 26);
                Total_integrity_TX_NEW = RetrieveExcelValueUsingInterop(sht, 17, 26);
                Total_integrity_TX_Curr = RetrieveExcelValueUsingInterop(sht, 18, 26);

                Total_Validity_HTC_TST = RetrieveExcelValueUsingInterop(sht, 8, 27);
                Total_Validity_PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 12, 27);
                Total_Validity_PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 15, 27);
                Total_Validity_PMTCT_ART = RetrieveExcelValueUsingInterop(sht, 16, 27);
                Total_Validity_TX_NEW = RetrieveExcelValueUsingInterop(sht, 17, 27);
                Total_Validity_TX_Curr = RetrieveExcelValueUsingInterop(sht, 18, 27);

                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43}",
                    FacilityName.Replace(",", "-"), FacilityCode, HTC_Charts, Total_Completeness_HTC_TST, PMTCT_STAT_charts, Total_Completeness_PMTCT_STAT, PMTCT_EID_charts, Total_completeness_PMTCT_EID, PMTCT_ARV_Charts, Total_completeness_PMTCT_ARV, TX_NEW_charts, Total_completeness_TX_NEW, TX_CURR_charts, Total_completeness_TX_CURR, Total_consistency_HTC_TST, Total_consistency_PMTCT_STAT, Total_consistency_PMTCT_EID, Total_consistency_PMTCT_ART, Total_consistency_TX_NEW, Total_consistency_TX_Curr, HTC_Charts_Precisions, Total_precision_HTC_TST, PMTCT_STAT_Charts_Precisions, Total_precision_PMTCT_STAT, PMTCT_EID_Charts_Precisions, Total_precision_PMTCT_EID, PMTCT_ARV_Charts_Precisions, Total_precision_PMTCT_ARV, TX_NEW_Charts_Precisions, Total_precision_TX_NEW, TX_CURR_Charts_Precisions, Total_precision_TX_CURR, Total_integrity_HTC_TST, Total_integrity_PMTCT_STAT, Total_integrity_PMTCT_EID, Total_integrity_PMTCT_ART, Total_integrity_TX_NEW, Total_integrity_TX_Curr, Total_Validity_HTC_TST, Total_Validity_PMTCT_STAT, Total_Validity_PMTCT_EID, Total_Validity_PMTCT_ART, Total_Validity_TX_NEW, Total_Validity_TX_Curr));

                wkb.Close(false, Missing.Value, Missing.Value);
            }
            xApp.Quit();

            File.WriteAllText(@"C:\Users\cmadubuko\Desktop\DQA Datim source\DQADimensions.csv", sb.ToString());
            Console.WriteLine("Press enter to end");
            return;
        }

        private void RetrieveExcelValue()
        {
            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Project\ShieldPortal\DMP\Report\Downloads\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();

            Excel.Application xApp = new Excel.Application();
            var oldAlert = xApp.DisplayAlerts;
            xApp.DisplayAlerts = false;
            foreach (var file in files)
            {
                string Datim_HTC_TST = "";
                string DATIM_HTC_TST_POS = "";
                string DATIM_HTC_ONLY = "";
                string DATIM_HTC_POS = "";
                string DATIM_PMTCT_STAT = "";
                string DATIM_PMTCT_STAT_POS = "";
                string DATIM_PMTCT_STAT_Previously = "";
                string DATIM_PMTCT_EID = "";
                string DATIM_PMTCT_ART = "";
                string Datim_TX_NEW = "";
                string DATIM_TX_CURR = "";

                string HTC_TST = "";
                string HTC_TST_POS = "";
                string HTC_ONLY = "";
                string HTC_POS = "";
                string PMTCT_STAT = "";
                string PMTCT_STAT_POS = "";
                string PMTCT_STAT_Previoulsy_Known = "";
                string PMTCT_EID = "";
                string PMTCT_ART = "";
                string TX_NEW = "";
                string TX_Curr = "";
                string FacilityName = "", FacilityCode = "";

                var wkb = xApp.Workbooks.Open(file, 0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true); //(file);
                Excel.Worksheet sht = wkb.Sheets["Worksheet"];
                FacilityName = RetrieveExcelValueUsingInterop(sht, 2, 22);
                FacilityCode = RetrieveExcelValueUsingInterop(sht, 2, 27); //sht.Cells["AA2"].Value;

                sht = wkb.Sheets["All Questions"];
                Datim_HTC_TST = RetrieveExcelValueUsingInterop(sht, 2, 6); //sht.Cells["F2"].Value;
                DATIM_PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 46, 6); //sht.Cells["F46"].Value;
                DATIM_PMTCT_ART = RetrieveExcelValueUsingInterop(sht, 91, 6); //sht.Cells["F91"].Value;
                Datim_TX_NEW = RetrieveExcelValueUsingInterop(sht, 141, 6); //sht.Cells["F141"].Value;
                DATIM_TX_CURR = RetrieveExcelValueUsingInterop(sht, 166, 6);// sht.Cells["F166"].Value;
                DATIM_PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 117, 6); //sht.Cells["F117"].Value;
                DATIM_HTC_ONLY = !string.IsNullOrEmpty(Datim_HTC_TST) && !string.IsNullOrEmpty(DATIM_PMTCT_STAT) ? (Convert.ToInt32(Datim_HTC_TST) - Convert.ToInt32(DATIM_PMTCT_STAT)).ToString() : " ";

                sht = wkb.Sheets["DQA Summary (Map to Quest Ans)"];
                HTC_TST = RetrieveExcelValueUsingInterop(sht, 8, 22);
                HTC_TST_POS = RetrieveExcelValueUsingInterop(sht, 9, 22);
                HTC_ONLY = RetrieveExcelValueUsingInterop(sht, 10, 22);
                HTC_POS = RetrieveExcelValueUsingInterop(sht, 11, 22);
                PMTCT_STAT = RetrieveExcelValueUsingInterop(sht, 12, 22);
                PMTCT_STAT_POS = RetrieveExcelValueUsingInterop(sht, 13, 22);
                PMTCT_STAT_Previoulsy_Known = RetrieveExcelValueUsingInterop(sht, 14, 22);
                PMTCT_EID = RetrieveExcelValueUsingInterop(sht, 15, 22);
                PMTCT_ART = RetrieveExcelValueUsingInterop(sht, 16, 22);
                TX_NEW = RetrieveExcelValueUsingInterop(sht, 17, 22);
                TX_Curr = RetrieveExcelValueUsingInterop(sht, 18, 22);

                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}",
                    FacilityName.Replace(",", "-"), FacilityCode, Datim_HTC_TST, HTC_TST, DATIM_HTC_TST_POS, HTC_TST_POS, DATIM_HTC_ONLY, HTC_ONLY, DATIM_HTC_POS, HTC_POS, DATIM_PMTCT_STAT, PMTCT_STAT, DATIM_PMTCT_STAT_POS, PMTCT_STAT_POS, DATIM_PMTCT_STAT_Previously, PMTCT_STAT_Previoulsy_Known, DATIM_PMTCT_EID, PMTCT_EID, DATIM_PMTCT_ART, PMTCT_ART, Datim_TX_NEW, TX_NEW, DATIM_TX_CURR, TX_Curr));

                wkb.Close(false, Missing.Value, Missing.Value);
            }

            xApp.Quit();

            File.WriteAllText(@"C:\Users\cmadubuko\Desktop\DQA Datim source\New_DQAComparison.csv", sb.ToString());
            Console.WriteLine("Press enter to end");
            return;
        }


        /// <summary>
        /// Retrieve excel value using Microsoft.Office.Interop.Excel;
        /// Use on local system only
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string RetrieveExcelValueUsingInterop(Excel.Worksheet sheet, int row, int column)
        {
            Excel.Range range = (Excel.Range)sheet.Cells[row, column];
            return Convert.ToString(range.Value2);
        }

        static string makeCSVEntry(string incoming)
        {
            if (string.IsNullOrEmpty(incoming))
            {
                return ",";
            }
            else
            {
                return string.Format("\"{0}\",", incoming.Trim().Replace(",", "|"));
            }
        }

        static void DMPSiteType()
        {
            IList<DMPDocument> dmps = null;
            try
            {
                dmps = new DMPDocumentDAO().RetrieveAll();
            }
            catch (Exception ex)
            {
                var exp = ex.InnerException as ReflectionTypeLoadException;
                if (exp != null)
                {
                    var le = exp.LoaderExceptions;
                }
            }
            StringBuilder siteno = new StringBuilder();
            siteno.AppendLine("IP, ART, PMTCT, HTC, OVC, Community");
            StringBuilder sb_dr = new StringBuilder();
            sb_dr.AppendLine("IP, Version Date, Version Number, Title of Author, Name of Author, Job Designation of Author, Phone No of Author, Email of Author, Title of Approver, Name of Approver, Job Designation of Approver, Phone no. of Approver, Email of Approver");

            foreach (var dmp in dmps)
            {
                var tt = dmp.Document.MonitoringAndEvaluationSystems.Environment.NumberOfSitesCoveredByImplementingPartners;
                string anEntry = string.Format("{0}, {1}, {2}, {3}, {4}, {5} ", dmp.TheDMP.Organization.ShortName, tt.ART, tt.PMTCT, tt.HTC, tt.OVC, tt.Commmunity);
                siteno.AppendLine(anEntry);

                var version = dmp.Document.DocumentRevisions.LastOrDefault().Version;
                anEntry = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
                    dmp.TheDMP.Organization.ShortName, version.VersionMetadata.VersionDate, version.VersionMetadata.VersionNumber, version.VersionAuthor.TitleOfAuthor, version.VersionAuthor.DisplayName, version.VersionAuthor.JobDesignation, version.VersionAuthor.PhoneNumberOfAuthor, version.VersionAuthor.EmailAddressOfAuthor, version.Approval.TitleofApprover, version.Approval.DisplayName, version.Approval.JobdesignationApprover, version.Approval.PhonenumberofApprover, version.Approval.EmailaddressofApprover);
                sb_dr.AppendLine(anEntry);
            }
            StringBuilder sb_tr = new StringBuilder();
            foreach (var dmp in dmps)
            {
                sb_tr.AppendLine(dmp.TheDMP.Organization.ShortName);
                sb_tr.AppendLine("Name of Training, Workstation (Site), Workstation (Region/State), Workstation (HQ)");
                foreach (var tr in dmp.Document.MonitoringAndEvaluationSystems.People.Trainings)
                {
                    string anEntry = string.Format("{0}, {1}, {2}, {3}", tr.NameOfTraining, tr.SiteDisplayDate, tr.RegionDisplayDate, tr.HQDisplayDate);
                    sb_tr.AppendLine(anEntry);
                }
            }

            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpSiteSummary.csv", siteno.ToString());
            File.AppendAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpSiteSummary.csv", sb_tr.ToString());
            File.AppendAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpSiteSummary.csv", sb_dr.ToString());
        }


        static void DMPSummary()
        {
            var dmps = new DMPDocumentDAO().RetrieveAll();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("IP,Project Title,Document Title,Organization,Lead Activity Manager,Address of Organization,	Mission Partner,Project Start Date,	Project End Date,Grant Reference Number,Ethical approval for the project,Rational,Aprroving instititional review board,Type of ethical approval,VERSION AUTHOR,VERSION NUMBER,VERSION DATE,	APPROVAL,Program Objective,	Roles,Number of staff site,	Number of staff state,	Number of staff HQ,	Responsibilities, Responsibility at site,Responsibility at State,	Responsibility at HQ,Name of training,Implementing partner M & E process,Report level,Data collation types,	Data collation frequency,Data garnering,Data use,Data improvement approach,	Project equipments,	States covered by implementing partners,Contracts and agreements,Ownership,Use of third party data sources,Data To Retain,Pre-Existing Data,Duration,Licensing,Digital Data Retention,Non Digital Data Retention"); //others

            StringBuilder sb_dcollection = new StringBuilder();
            sb_dcollection.AppendLine("IP,Reporting Level, Data Type, Reporting Tools, Collection process");//data collection 

            StringBuilder sb_report = new StringBuilder();
            sb_report.AppendLine("IP,Reporting Level,Reports Type,Reported To,Program Area,Frequency of Reporting,Duration of Reporting,Timelines");// report

            StringBuilder sb_dverification = new StringBuilder();
            sb_dverification.AppendLine("IP,Reporting Level,ThematicArea,DataVerificationApproach,Types,Frequency,Duration,Timelines"); //dverification

            StringBuilder sb_digitaldata = new StringBuilder();
            sb_digitaldata.AppendLine("IP,Reporting Level,Thematic Area,Volume of digital data, Data storage format,Storage location,Backup,Data security, Patient confidentiality policies,Storage of pre existing data"); //digitaldata

            StringBuilder sb_non_digital_data = new StringBuilder();
            sb_non_digital_data.AppendLine("IP,Reporting Level,Thematic Area, data types,Storage location,SafeguardsAndRequirements"); //non digital data

            StringBuilder sb_sharing = new StringBuilder();
            sb_sharing.AppendLine("IP,Reporting Level,Thematic Area,Data access, Sharing policies,Data transmission policies,Sharing flatforms"); //sharing

            StringBuilder sb_doc_mgt = new StringBuilder();
            sb_doc_mgt.AppendLine("IP,Reporting Level,Thematic Area,Documentation and dataDescriptors, Naming structure and filing structures"); //doc mgt

            foreach (var dmp in dmps)
            {
                string anEntry = "";
                var doc = dmp.Document;
                var docRev = doc.DocumentRevisions.LastOrDefault();

                ////data doc mgt
                foreach (var dv in doc.DataStorageAccessAndSharing.DataDocumentationManagementAndEntry)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(dv.ReportingLevel);
                    anEntry += makeCSVEntry(dv.ThematicArea);
                    anEntry += makeCSVEntry(dv.StoredDocumentationAndDataDescriptors);
                    anEntry += makeCSVEntry(dv.NamingStructureAndFilingStructures);
                    sb_doc_mgt.AppendLine(anEntry);
                }

                ////data sharing
                foreach (var dv in doc.DataStorageAccessAndSharing.DataAccessAndSharing)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(dv.ReportingLevel);
                    anEntry += makeCSVEntry(dv.ThematicArea);
                    anEntry += makeCSVEntry(dv.DataAccess);
                    anEntry += makeCSVEntry(dv.DataSharingPolicies);
                    anEntry += makeCSVEntry(dv.DataTransmissionPolicies);
                    anEntry += makeCSVEntry(dv.SharingPlatForms);
                    sb_sharing.AppendLine(anEntry);
                }

                ////nondigital
                foreach (var dv in doc.DataStorageAccessAndSharing.NonDigital)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(dv.ReportingLevel);
                    anEntry += makeCSVEntry(dv.ThematicArea);
                    anEntry += makeCSVEntry(dv.NonDigitalDataTypes);
                    anEntry += makeCSVEntry(dv.StorageLocation);
                    anEntry += makeCSVEntry(dv.SafeguardsAndRequirements);
                    sb_non_digital_data.AppendLine(anEntry);
                }

                //digital data
                foreach (var dv in doc.DataStorageAccessAndSharing.Digital)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(dv.ReportingLevel);
                    anEntry += makeCSVEntry(dv.ThematicArea);
                    anEntry += makeCSVEntry(dv.VolumeOfDigitalData);
                    anEntry += makeCSVEntry(dv.DataStorageFormat);
                    anEntry += makeCSVEntry(dv.StorageLocation);
                    anEntry += makeCSVEntry(dv.Backup);
                    anEntry += makeCSVEntry(dv.DataSecurity);
                    anEntry += makeCSVEntry(dv.PatientConfidentialityPolicies);
                    anEntry += makeCSVEntry(dv.StorageOfPreExistingData);
                    sb_digitaldata.AppendLine(anEntry);
                }


                //dverification
                foreach (var dv in doc.QualityAssurance.DataVerification)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(dv.ReportingLevel);
                    anEntry += makeCSVEntry(dv.ThematicArea);
                    anEntry += makeCSVEntry(dv.DataVerificationApproach);
                    anEntry += makeCSVEntry(dv.TypesOfDataVerification);
                    anEntry += makeCSVEntry(dv.FrequencyOfDataVerification);
                    anEntry += dv.DurationOfDataVerificaion + ",";
                    anEntry += makeCSVEntry(string.Join(System.Environment.NewLine, dv.TimelinesForDataVerification.Select(x => x)));
                    sb_dverification.AppendLine(anEntry);
                }


                //report
                foreach (var rt in doc.DataProcesses.Reports.ReportData)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(rt.ReportingLevel);
                    anEntry += makeCSVEntry(rt.ReportsType);
                    anEntry += makeCSVEntry(rt.ReportedTo);
                    anEntry += makeCSVEntry(rt.ProgramArea);
                    anEntry += makeCSVEntry(rt.FrequencyOfReporting);
                    anEntry += rt.DurationOfReporting + ",";
                    anEntry += makeCSVEntry(string.Join(System.Environment.NewLine, rt.TimelinesForReporting.Select(x => x)));
                    sb_report.AppendLine(anEntry);
                }

                //data collection             
                foreach (var dt in doc.DataProcesses.DataCollection)
                {
                    anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                    anEntry += makeCSVEntry(dt.ReportingLevel);
                    anEntry += makeCSVEntry(dt.DataType);
                    anEntry += makeCSVEntry(dt.DataCollectionAndReportingTools);
                    anEntry += makeCSVEntry(dt.DataCollectionProcess);
                    sb_dcollection.AppendLine(anEntry);
                }

                anEntry = makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                anEntry += makeCSVEntry(dmp.TheDMP.TheProject.ProjectTitle);
                anEntry += makeCSVEntry(dmp.TheDMP.DMPTitle);
                anEntry += makeCSVEntry(dmp.TheDMP.Organization.ShortName);
                anEntry += makeCSVEntry(dmp.TheDMP.TheProject.LeadActivityManager != null ? dmp.TheDMP.TheProject.LeadActivityManager.FullName : "");
                anEntry += makeCSVEntry(dmp.TheDMP.Organization.Address);
                anEntry += makeCSVEntry(dmp.TheDMP.Organization.MissionPartner);
                anEntry += makeCSVEntry(dmp.TheDMP.TheProject.ProjectStartDate);
                anEntry += makeCSVEntry(dmp.TheDMP.TheProject.ProjectEndDate);
                anEntry += makeCSVEntry(dmp.TheDMP.TheProject.GrantReferenceNumber);
                anEntry += makeCSVEntry(doc.ProjectProfile.EthicalApproval.EthicalApprovalForTheProject);
                anEntry += makeCSVEntry(doc.ProjectProfile.EthicalApproval.Rational);
                anEntry += makeCSVEntry(doc.ProjectProfile.EthicalApproval.AprrovingInstititionalReviewBoard);
                anEntry += makeCSVEntry(doc.ProjectProfile.EthicalApproval.TypeOfEthicalApproval);
                anEntry += makeCSVEntry(docRev != null && docRev.Version != null ? docRev.Version.VersionAuthor.DisplayName : "");
                anEntry += makeCSVEntry(docRev != null && docRev.Version != null ? docRev.Version.VersionMetadata.VersionNumber : "");
                anEntry += makeCSVEntry(docRev != null && docRev.Version != null ? docRev.Version.VersionMetadata.VersionDate : "");
                anEntry += makeCSVEntry(docRev != null && docRev.Version != null && docRev.Version.Approval != null ? docRev.Version.Approval.DisplayName : "");
                anEntry += makeCSVEntry(doc.Planning.Summary.ProjectObjectives);
                anEntry += makeCSVEntry(string.Join("|", doc.MonitoringAndEvaluationSystems.People.Roles.Select(x => x.Name)));
                anEntry += doc.MonitoringAndEvaluationSystems.People.Roles.Sum(x => x.SiteCount) + ",";
                anEntry += doc.MonitoringAndEvaluationSystems.People.Roles.Sum(x => x.RegionCount) + ",";
                anEntry += doc.MonitoringAndEvaluationSystems.People.Roles.Sum(x => x.HQCount) + ",";
                anEntry += makeCSVEntry(string.Join("|", doc.MonitoringAndEvaluationSystems.People.Responsibilities.Select(x => x.Name)));
                anEntry += doc.MonitoringAndEvaluationSystems.People.Responsibilities.Sum(x => x.SiteCount) + ",";
                anEntry += doc.MonitoringAndEvaluationSystems.People.Responsibilities.Sum(x => x.RegionCount) + ",";
                anEntry += doc.MonitoringAndEvaluationSystems.People.Responsibilities.Sum(x => x.HQCount) + ",";
                anEntry += makeCSVEntry(string.Join("|", doc.MonitoringAndEvaluationSystems.People.Trainings.Select(x => x.NameOfTraining)));
                //anEntry += makeCSVEntry(doc.MonitoringAndEvaluationSystems.Process.ImplementingPartnerMEProcess);
                anEntry += makeCSVEntry(string.Join("|", doc.MonitoringAndEvaluationSystems.Process.ReportLevel.Select(x => x)));
                anEntry += makeCSVEntry(string.Join("|", doc.MonitoringAndEvaluationSystems.Process.DataCollation.Select(x => x.DataType)));
                anEntry += makeCSVEntry(string.Join("|", doc.MonitoringAndEvaluationSystems.Process.DataCollation.Select(x => x.CollationFrequency)));
                anEntry += makeCSVEntry(doc.MonitoringAndEvaluationSystems.Process.DataGarnering);
                anEntry += makeCSVEntry(doc.MonitoringAndEvaluationSystems.Process.DataUse);
                anEntry += makeCSVEntry(doc.MonitoringAndEvaluationSystems.Process.DataImprovementApproach);
                anEntry += makeCSVEntry(doc.MonitoringAndEvaluationSystems.Equipment.ProjectEquipments);
                anEntry += makeCSVEntry(doc.MonitoringAndEvaluationSystems.Environment.StatesCoveredByImplementingPartners);

                anEntry += makeCSVEntry(doc.IntellectualPropertyCopyrightAndOwnership.ContractsAndAgreements);
                anEntry += makeCSVEntry(doc.IntellectualPropertyCopyrightAndOwnership.Ownership);
                anEntry += makeCSVEntry(doc.IntellectualPropertyCopyrightAndOwnership.UseOfThirdPartyDataSources);

                anEntry += makeCSVEntry(doc.PostProjectDataRetentionSharingAndDestruction.DataToRetain);
                anEntry += makeCSVEntry(doc.PostProjectDataRetentionSharingAndDestruction.PreExistingData);
                anEntry += makeCSVEntry(doc.PostProjectDataRetentionSharingAndDestruction.Duration);
                anEntry += makeCSVEntry(doc.PostProjectDataRetentionSharingAndDestruction.Licensing);
                anEntry += makeCSVEntry(doc.PostProjectDataRetentionSharingAndDestruction.DigitalDataRetention.DataRetention);
                anEntry += makeCSVEntry(doc.PostProjectDataRetentionSharingAndDestruction.NonDigitalRentention.DataRention);
                sb.AppendLine(anEntry);
            }
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary.csv", sb.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_dcollection.csv", sb_dcollection.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_digitaldata.csv", sb_digitaldata.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_doc_mgt.csv", sb_doc_mgt.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_dverification.csv", sb_dverification.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_non_digital_data.csv", sb_non_digital_data.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_report.csv", sb_report.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DMP\DMP summary\_dmpsummary_sharing.csv", sb_sharing.ToString());
        }

        public void GenerateNonDatimCode()
        {
            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\December 31 2016\December 31 2016\";// @"C:\Users\cmadubuko\Google Drive\MGIC\Project\ShieldPortal\Test\sample biweekly files\";
            string[] files = Directory.GetFiles(baseLocation, "*_new.xlsx", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                int count = 0;
                string newTemplate = file.Replace("_new.xlsx", "_template.xlsx");
                string existingTemplate = file;
                using (ExcelPackage package = new ExcelPackage(new FileInfo(existingTemplate)))
                {
                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string lgaName = sheet.Name;
                        if (lgaName.ToLower().Contains("dashboard") || lgaName.Contains("LGA Level Dashboard"))
                            continue;

                        for (int row = 8; ; row++)
                        {
                            string fName = (string)sheet.Cells[row, 2].Value;
                            if (!string.IsNullOrEmpty(fName))
                            {
                                string code = (string)sheet.Cells[row, 1].Value;
                                if (string.IsNullOrEmpty(code))
                                {
                                    string[] ip = file.Split(new string[] { @"\", "_new.xlsx" }, StringSplitOptions.RemoveEmptyEntries);
                                    count += 1;
                                    sheet.Cells[row, 1].Value = string.Format("M{0}{1}", ip[ip.Count() - 1], count.ToString().PadLeft(3, '0'));
                                }

                            }
                            else
                                break;
                        }
                    }
                    package.SaveAs(new FileInfo(newTemplate));
                }
            }
        }

        public void GenerateFacilityCode()
        {
            YearlyPerformanceTargetDAO yptDAO = new YearlyPerformanceTargetDAO();

            // HealthFacilityDAO _sdfDao = new HealthFacilityDAO();
            // var IndexPeriods = ExcelHelper.GenerateIndexedPeriods();
            //var LGADictionary = new LGADao().RetrieveAll().ToDictionary(x => x.lga_code);

            //var ypts = yptDAO.GenerateYearlyTargetGroupedByLGA(2017);
            // var facilitiesGroupedByLGA = _sdfDao.RetrieveAll().GroupBy(x => x.LGA.lga_name).ToList();


            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\December 31 2016\December 31 2016\";// @"C:\Users\cmadubuko\Google Drive\MGIC\Project\ShieldPortal\Test\sample biweekly files\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.TopDirectoryOnly);

            var masterList = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\Reconciliation\FacilityWithCodes.csv";
            string[] linesInmasterList = File.ReadAllText(masterList).Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (var file in files)
            {
                string newTemplate = file.Replace(".xlsx", "_new.xlsx");
                string existingTemplate = file;
                using (ExcelPackage package = new ExcelPackage(new FileInfo(existingTemplate)))
                {
                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string lgaName = sheet.Name;
                        if (lgaName.ToLower().Contains("dashboard") || lgaName.Contains("LGA Level Dashboard"))
                            continue;

                        for (int row = 8; ; row++)
                        {
                            string fName = (string)sheet.Cells[row, 2].Value;
                            if (!string.IsNullOrEmpty(fName))
                            {
                                string code = SearchMasterList(linesInmasterList, fName, lgaName);
                                sheet.Cells[row, 1].Value = code;
                                if (code == "")
                                {
                                    sb.AppendLine(fName + "," + lgaName + "," + file);
                                }
                            }
                            else
                                break;
                        }
                    }
                    //foreach (var item in facilitiesGroupedByLGA)
                    //{
                    //    if (ypts.FirstOrDefault(x => x.lga_name == item.Key) == null)
                    //        continue;

                    //    string lgaName = item.Key;

                    //    ExcelWorksheet sheet = RetrieveMatchingWorkSheet(lgaName, package.Workbook.Worksheets, LGADictionary);
                    //    if (sheet == null)
                    //        continue;
                    //    var facilitiesInLGA = item.ToList();

                    //for (int row = 8; ; row++)  
                    //{
                    //    string fName = (string)sheet.Cells[row, 3].Value;
                    //    if (!string.IsNullOrEmpty(fName))
                    //    {
                    //        var sdf = facilitiesInLGA.FirstOrDefault(x => x.Name.ToLower().Trim() == fName);


                    //        if (sdf != null)
                    //        {
                    //            sheet.Cells[row, 1].Value = sdf.FacilityCode;
                    //        }
                    //    }
                    //    else
                    //        break;
                    //}
                    //}
                    package.SaveAs(new FileInfo(newTemplate));
                }
            }
            File.WriteAllText(baseLocation + "_notFound.csv", sb.ToString());
        }

        private string SearchMasterList(string[] lines, string fname, string lgaName)
        {
            if (lgaName.ToLower().Contains("ifako"))
                lgaName = "Ifako Ijaiye";

            if (lgaName.ToLower().Contains("amac"))
                lgaName = "Municipal Area Council";

            foreach (var line in lines)
            {
                string[] items = line.Split(',');
                if (string.IsNullOrEmpty(items[0]))
                    break;

                string Sname = items[0].Trim().ToLower().Replace("-", "").Replace(",", "");
                string Slganame = items[6].Trim().ToLower();
                if (Sname == fname.Trim().ToLower().Replace("-", "").Replace(",", "") && lgaName.Trim().ToLower() == Slganame)
                    return items[2];
            }
            return "";
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

        private void GenerateFacilityTargetCSV()
        {
            var hfDAO = new HealthFacilityDAO();
            var yptDAO = new YearlyPerformanceTargetDAO();
            StringBuilder invalid = new StringBuilder();
            StringBuilder valid = new StringBuilder();

            List<YearlyPerformanceTarget> ypts = new List<YearlyPerformanceTarget>();

            valid.AppendLine("facilityName, facilityCode, HTC_TST, HTC_TST_pos, Tx_NEW");
            invalid.AppendLine("facilityName, facilityCode, HTC_TST, HTC_TST_pos, Tx_NEW");

            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\December 31 2016\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.TopDirectoryOnly);

            var existingFacilities = hfDAO.RetrieveAll();//.Where(x => file.Split('_')[0] == x.Organization.ShortName);//.ToDictionary(x => x.Name);

            foreach (var file in files)
            {
                string ip = file.Split(new string[] { @"\" }, StringSplitOptions.None)[8].Split('_')[0];

                invalid.AppendLine(file);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string name = sheet.Name;
                        if (name.ToLower().Contains("dashboard") || name.Contains("LGA Level Dashboard"))
                            continue;

                        int row = 8;

                        while (true)
                        {
                            string facilityName = ExcelHelper.ReadCell(sheet, row, 2);
                            string facilitycode = ExcelHelper.ReadCell(sheet, row, 1);
                            if (string.IsNullOrEmpty(facilityName))
                            {
                                break;
                            }

                            HealthFacility theFacility = null;
                            theFacility = existingFacilities.FirstOrDefault(x => x.FacilityCode.Trim() == facilitycode);

                            int HTC_TST = 0;
                            int HTC_TST_pos = 0;
                            int Tx_NEW = 0;
                            int.TryParse(ExcelHelper.ReadCell(sheet, row, 4), out HTC_TST);
                            int.TryParse(ExcelHelper.ReadCell(sheet, row, 6), out HTC_TST_pos);
                            int.TryParse(ExcelHelper.ReadCell(sheet, row, 8), out Tx_NEW);

                            if (theFacility == null)
                            {
                                invalid.AppendLine(string.Format("{0},{1},{2},{3},{4}, {5}", facilityName, "", HTC_TST, HTC_TST_pos, Tx_NEW, sheet.Name));
                            }
                            else
                            {
                                valid.AppendLine(string.Format("{0},{1},{2},{3},{4}, {5}", facilityName.Replace(',', '-'), theFacility.FacilityCode, HTC_TST, HTC_TST_pos, Tx_NEW, theFacility.LGA.DisplayName));

                                ypts.Add(new YearlyPerformanceTarget
                                {
                                    FiscalYear = 2017,
                                    HealthFacilty = theFacility,
                                    HTC_TST = HTC_TST,
                                    HTC_TST_POS = HTC_TST_pos,
                                    Tx_NEW = Tx_NEW,
                                });
                            }
                            row++;
                        }
                    }
                }
            }
            // yptDAO.BulkInsert(ypts);
            File.WriteAllText(baseLocation + "_notFound.csv", invalid.ToString());
            File.WriteAllText(baseLocation + "_Found.csv", valid.ToString());
            Console.WriteLine("press enter to continue");
            Console.ReadLine();
        }


        //private async void UpdateFacilities()
        //{
        //    HealthFacilityDAO hfdao = new HealthFacilityDAO();
        //    var hfs = hfdao.RetrieveAll();
        //    StringBuilder sb = new StringBuilder();
        //    var taskList = new List<Task<string>>();

        //    foreach (var hf in hfs)
        //    {
        //        string baseUrl = "https://www.datim.org/api/organisationUnits/" + hf.FacilityCode + "?fields=coordinates";
        //        taskList.Add(RetrieveDatimData(baseUrl, hf));
        //    }

        //    var result = await Task.WhenAll(taskList);
        //    string query = string.Join(";", result);
        //    Console.WriteLine("Finish fetching data. Press enter to update the db");
        //    //Console.ReadLine();
        //    DirectUpdateDB(query);
        //}

        private async Task<string> RetrieveDatimData(string uri, HealthFacility hf)
        {
            string script = "";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("UMB_SHIELD:UMB@sh1eld")));
                httpClient.Timeout = TimeSpan.FromMinutes(2);
                await httpClient.GetAsync(uri);

                //await httpClient.GetAsync(uri)
                //  .ContinueWith(x =>
                // {
                //     Console.WriteLine("generated script for hf :{0}", uri);
                //     if (x.IsCompleted && x.Status == TaskStatus.RanToCompletion)
                //     {
                //         var dt = x.Result.Content.ReadAsAsync<DatimResponse>().Result;
                //         if (dt != null && !string.IsNullOrEmpty(dt.coordinates))
                //         {
                //             string[] lnglat = dt.coordinates.Split(new string[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
                //             if (lnglat.Count() == 2)
                //             {
                //                 Console.WriteLine("generated script for hf :{0}", hf.Id);
                //                 script = string.Format("Update[HealthFacility] set longitude = '{0}', latitude = '{1}' where id = {2};", lnglat[0], lnglat[1], hf.Id);
                //             }
                //         }
                //     }
                // });
            }
            return script;
        }

        void DirectUpdateDB(string script)
        {
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand command = new SqlCommand(script, s);
                int rows = command.ExecuteNonQuery();
                command.Dispose();
                Console.WriteLine("{0} rows affected", rows);
            }
        }

        public void InsertFacility()
        {
            StreamReader reader = new StreamReader(@"C:\Users\Somadina Mbadiwe\AppData\Roaming\Skype\My Skype Received Files\Facility_List(1).csv");
            string header = reader.ReadLine();
            string[] content = reader.ReadToEnd().Split(new string[] { "\n\r", System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            reader.Close();
            reader.Dispose();
            HealthFacilityDAO hfdao = new HealthFacilityDAO();
            IList<LGA> lgas = new LGADao().RetrieveAll();
            IList<Organizations> orgs = new OrganizationDAO().RetrieveAll();

            foreach (var item in content)
            {
                string[] theline = item.Split(',');

                LGA lga = lgas.FirstOrDefault(x => x.lga_name.Trim().ToUpper() == theline[2].Trim().ToUpper() && x.State.state_name.Trim().ToUpper() == theline[1].Trim().ToUpper());
                if (lga == null)
                    continue;

                HealthFacility hf = new HealthFacility
                {
                    FacilityCode = theline[4],
                    Name = theline[3],
                    LGA = lga,
                    Organization = orgs.FirstOrDefault(x => x.ShortName == theline[5]),
                    OrganizationType = CommonUtil.Enums.OrganizationType.HealthFacilty
                };
                hfdao.Save(hf);
            }
            hfdao.CommitChanges();
        }

        void CopyAndPaste()
        {
            string destinationDir = @"C:\Users\cmadubuko\Desktop\DQAs\";
            string baseLocation = @"C:\Users\cmadubuko\Desktop\Uploads\";
            string doneLocation = @"C:\Users\cmadubuko\Desktop\Unproted doc\";

            var files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                string newTemplate = destinationDir + file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries)[5];
                string existingTemplate = doneLocation + file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries)[5];


                bool exist = File.Exists(existingTemplate);
                if (exist)
                    continue;

                if (!File.Exists(newTemplate))
                {
                    FileInfo info = new FileInfo(existingTemplate);
                    info.CopyTo(newTemplate);
                }
            }
        }

        void UnprotectExcelFile()
        {
            string destinationDir = @"C:\Users\cmadubuko\Desktop\DQAs\";
            string baseLocation = @"C:\Users\cmadubuko\Desktop\Uploads\";
            var files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            Excel.Application xApp = new Excel.Application();
            foreach (var file in files)
            {
                string newTemplate = destinationDir + file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries)[5];
                string existingTemplate = file;

                bool exist = File.Exists(newTemplate);
                if (exist)
                    continue;

                var wkb = xApp.Workbooks.Open(file, 0, false, 5, "Pa55w0rd1", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true);
                wkb.Unprotect("Pa55w0rd1");

                foreach (Excel.Worksheet sheet in wkb.Sheets)
                {
                    sheet.Unprotect("Pa55w0rd1");
                    sheet.Visible = Excel.XlSheetVisibility.xlSheetVisible;
                }
                wkb.SaveAs(newTemplate, wkb.FileFormat, "", "", false, false, Excel.XlSaveAsAccessMode.xlNoChange);
                wkb.Close();
            }
            xApp.Quit();
        }

        public void QuerySystem()
        {
            var dmpDao = new DMPDocumentDAO();
            dmpDao.GenericSearch("maryland global");
            //var yy = dmpDao.Retrieve(1);

            Console.ReadLine();
        }

    }

    public class DatimResponse
    {
        public string coordinates { get; set; }
    }

    public class DatimResponseIndicators
    {
        public string HTS { get; set; }
        public string PMTCT_STAT { get; set; }
        public string TX_NEW { get; set; }
        public string TX_Curr { get; set; }
        public string PMTCT_Eid { get; set; }
        public string PMTCT_Art { get; set; }

    }

}
