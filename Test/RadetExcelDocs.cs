using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class RadetExcelDocs
    {
       public static string ReturnLGA_n_State(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            var lgas = new LGADao().RetrieveAll();
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fileName)))
            {
                var sheet = package.Workbook.Worksheets["StateLGA"];
                var query3 = (from c in sheet.Cells 
                              where c.Value !=null &&c.Value.ToString().Contains("Local Government Area")
                              select c);
                foreach (var cell in query3)   
                {
                   // Console.WriteLine("Cell {0} has value {1:N0}", cell.Address, cell.Value);
                    if (FindLGA(lgas,cell.Value.ToString()) ==null) // lgas.FirstOrDefault(x => x.lga_name.ToLower() == cell.Value.ToString().ToLower().Substring(3).Replace(" local government area", "")) == null)
                    {
                        sb.AppendLine(string.Format("{0},", cell.Value));
                    }
                    
                } 
            }
            File.WriteAllText(@"C:\MGIC\Radet extracted\Live Radet\All IPs Unzipped\info_err.csv", sb.ToString());
            return sb.ToString();
        }

       static LGA FindLGA(IList<LGA> lgas, string lgaName)
        {
            var lga = lgas.FirstOrDefault(x => x.lga_name.ToLower() == lgaName.ToLower().Substring(3).Replace(" local government area", ""));
            if(lga == null)
            {
                lga = lgas.FirstOrDefault(x => !string.IsNullOrEmpty(x.alternative_name) && x.alternative_name.ToLower() == lgaName.ToLower().Substring(3).Replace(" local government area", ""));
            }
            return lga;
        }

        public static void ReadAndMerge()
        {
            UpdateRadetReport();


            return;
        }

        static void UpdateRadetReport()
        {
            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DQA\Live Radet\All IPs Unzipped\";
            StringBuilder sb = new StringBuilder();
            StringBuilder err = new StringBuilder();
            ConcurrentBag<string> files = new ConcurrentBag<string>(Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.AllDirectories));

            LGADao dao = new CommonUtil.DAO.LGADao();
            var LGAs = dao.RetrieveAll();

            files.AsParallel().ForAll(file =>
            //foreach(var file in files)
            {
                Console.WriteLine(file);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    LGA aLga = null;
                    State st = null;
                    var sheet = package.Workbook.Worksheets["MainPage"];
                    if (sheet == null)
                    {
                        err.AppendLine(string.Format("invalid file = '{0}'", file));
                    }
                    else
                    {
                        var stateObj = sheet.Cells["S14"].Value;

                        var slga = sheet.Cells["S17"].Value;
                        var facility = sheet.Cells["S20"].Value; 
                        if (stateObj != null && slga != null && facility !=null)
                        {
                            facility= facility.ToString().Substring(3);

                            string _l = slga.ToString().Trim().Substring(3).Replace(" Local Government Area", "");
                            var _tempLgas = LGAs.Where(x => x.lga_name == _l).ToList();
                            if (_tempLgas.Count == 1)
                            {
                                aLga = _tempLgas.FirstOrDefault();
                            }
                            else if (_tempLgas.Count > 1)
                            {
                                aLga = _tempLgas.FirstOrDefault(x => x.State.state_name == stateObj.ToString().Trim().Substring(3));
                            }
                            else if (_tempLgas != null && _tempLgas.Count == 0)
                            {
                                err.AppendLine(string.Format("LGA Name = '{0}', Facility = '{1}'", _l, facility));
                                //throw new ApplicationException(" Unable to locate the LGA");
                            }
                            //aLga =  LGAs.FirstOrDefault(x => x.Key == (stateObj.ToString().Trim().Substring(3))).ToList().FirstOrDefault(x => x.lga_name == _l);
                        }

                        if (aLga != null)
                        {
                            sb.AppendLine(string.Format("update dqa_radet_upload_report set LGA_code = '{0}' where Facility = '{1}';", aLga.lga_code, facility));
                        }
                    }
                }
            }
            );
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DQA\Live Radet\All IPs Unzipped\err.csv", err.ToString());
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DQA\Live Radet\All IPs Unzipped\Update.sql", sb.ToString());
        }

        private static void ReadAndMergeEnugu()
        {
            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\Akipu\Enugu_Line Lists_Mar 2017\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("S/N,Enrollment No,Hospital Number,DOB,Age,Sex,Date Enrolled into Care,WHO stage at enrollment,CD4 at enrollemnt,Clinical TB screening at enrollment ,ART Status,Date commenced into ART,Regimen at start of ART,Date regimen switched,Current Regimen,Q1 Clinic Visit Date,Q2 Clinic Visit Date,Q3 Clinic Visit Date,Q4 Clinic Visit Date,ART pick date 1,ART pick date 2,ART pick date 3,ART pick date 4,ART pick date 5,ART pick date 6,Patient Status,Date of Patient status,Exit Reason, State, Facility Name");
            foreach (var file in files)
            {
                Console.WriteLine(file);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {

                    var sheet = package.Workbook.Worksheets.FirstOrDefault();
                    string facilitName = ExcelHelper.ReadCellText(sheet, 4, 5);

                    int row = 7;

                    while (true)
                    {
                        string cellContent = ExcelHelper.ReadCellText(sheet, row, 2); //if no dob, end of file!!!
                        if (string.IsNullOrEmpty(cellContent))
                        {
                            break;
                        }
                        for (int col = 2; col <= 33; col++)
                        {
                            if (col == 4 || col == 5 || col == 6 || col == 13)
                            {
                                continue;
                            }
                            cellContent = ExcelHelper.ReadCellText(sheet, row, col);
                            sb.Append(cellContent + ",");
                        }
                        sb.Append("Enugu,");
                        sb.Append(facilitName);
                        sb.AppendLine();
                        row++;
                    }
                }
            }
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\Akipu\enugu.csv", sb.ToString());

        }


        private static void ReadAndMergeImo()
        {
            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\Akipu\Care lineList imo\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.TopDirectoryOnly);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("EnrollmentNo,PatientClinicId,DOB,Age,Sex,Patient Name,DateEnrolledintoCare,WHOStage at Enrollment,CD4 at Enrollment,ClinicalTBScreeningatEnrollment,ARTStartDate,RegimenAtStartOfART,Last documented WHO stage,Date of last documented WHO stage,Last Documented TB Screening,Date of last documented TB screening,Last documented CD4 Test,Date of last documented CD4 Test,Last documented Viral Load,Date of last documented Viral Load,LastVisitDate,LastPickupDate,Last Regimen dispensed,MonthsOfARVRefill,Visit in reporting period,ARV Pickup in reporting period,Regimen during reporting period,CTX during reporting period,INH during reporting period,WHO stage during reporting Period,Date of WHO stage during reporting Period,TB Screening during reporting period,Date of TB Screening during reporting period,CD4 Test Result during Reporting Period,Date of CD4 Test during reporting period,Viral Load during Reporting Period,Date of Viral Load during reporting period,ARTStatus,PatientStatus,DateofPatientStatus,ExitReason, State, Facility Name");
            foreach (var file in files)
            {
                Console.WriteLine(file);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var sheet = package.Workbook.Worksheets["Mar"];
                    int row = 2;
                    while (true)
                    {
                        string cellContent = ExcelHelper.ReadCell(sheet, row, 3); //if no dob, end of file!!!
                        if (string.IsNullOrEmpty(cellContent))
                        {
                            break;
                        }
                        for (int col = 1; col <= 41; col++)
                        {
                            cellContent = ExcelHelper.ReadCell(sheet, row, col);
                            sb.Append(cellContent + ",");
                        }
                        sb.Append("Imo,");
                        string[] fname = file.Split(new string[] { @"\", ".xlsx" }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(fname[fname.Count() - 1]);
                        sb.AppendLine();
                        row++;
                    }
                }
            }
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\Akipu\Imo.csv", sb.ToString());

        }

        private static void ReadAndMergeEbonyi()
        {
            string destFile = "eb_Care and Treatment LineList_March_2017.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("EnrollmentNo,PatientClinicId,DOB,Age,Sex,Patient Name,DateEnrolledintoCare,WHOStage at Enrollment,CD4 at Enrollment,ClinicalTBScreeningatEnrollment,ARTStartDate,RegimenAtStartOfART,Last documented WHO stage,Date of last documented WHO stage,Last Documented TB Screening,Date of last documented TB screening,Last documented CD4 Test,Date of last documented CD4 Test,Last documented Viral Load,Date of last documented Viral Load,LastVisitDate,LastPickupDate,Last Regimen dispensed,MonthsOfARVRefill,Visit in reporting period,ARV Pickup in reporting period,Regimen during reporting period,CTX during reporting period,INH during reporting period,WHO stage during reporting Period,Date of WHO stage during reporting Period,TB Screening during reporting period,Date of TB Screening during reporting period,CD4 Test Result during Reporting Period,Date of CD4 Test during reporting period,Viral Load during Reporting Period,Date of Viral Load during reporting period,ARTStatus,PatientStatus,DateofPatientStatus,ExitReason, State, Facility Name");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\Akipu\eb_Care and Treatment LineList_March_2017.xlsx")))
            {
                foreach (ExcelWorksheet sheet in package.Workbook.Worksheets)
                {
                    int row = 2;
                    // bool ctdLoop = true;
                    while (true)
                    {
                        string cellContent = ExcelHelper.ReadCell(sheet, row, 1);
                        if (string.IsNullOrEmpty(cellContent))
                        {
                            break;
                        }
                        for (int col = 1; col <= 41; col++)
                        {
                            cellContent = ExcelHelper.ReadCell(sheet, row, col);
                            sb.Append(cellContent + ",");
                        }
                        sb.Append("Ebonyi,");
                        sb.Append(sheet.Name);
                        sb.AppendLine();
                        row++;
                    }
                }
            }
            File.WriteAllText(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\Akipu\" + destFile, sb.ToString());
        }
    }
}
