using CommonUtil.Utilities;
using OfficeOpenXml;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Test
{
    public class MosisFiles
    {
        public static void ProcessHTS_TST_File()//
        {
            string outputDirectory = @"C:\Users\cmadubuko\Documents\Mosis\facilities\Imo HTS_TST Processed\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Enugu HTST_TST Processed\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi HTS_TST Processed\";

            string baseLocation_art_pmtct = @"C:\Users\cmadubuko\Documents\Mosis\facilities\Imo State _SEED_March 2017\Seeds\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Apr 2017_Enugu_Final\Apr 2017_Enugu_Final\Enugu SEEDS_Apr 2017\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi_Reports_March_2017\Ebonyi_Reports_March_2017\Seeds report\";

            string baseLocation_HTS_TST = @"C:\Users\cmadubuko\Documents\Mosis\facilities\Imo State _SEED_March 2017\Imo HTS_TST\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Apr 2017_Enugu_Final\Apr 2017_Enugu_Final\HTS_TST_Disaggregation_Report\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi_Reports_March_2017\Ebonyi_Reports_March_2017\HTS_TST_Ebonyi_10_3_2017";


            string[] hts_files = Directory.GetFiles(baseLocation_HTS_TST, "*.xlsx", SearchOption.AllDirectories);
            string[] art_pmtct_files = Directory.GetFiles(baseLocation_art_pmtct, "*.xlsx", SearchOption.AllDirectories);

            string[] mapping = File.ReadAllText(@"C:\Users\cmadubuko\Documents\Mosis\HTC _Mapping.csv").Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string template = @"C:\Users\cmadubuko\Documents\Mosis\HTS MSF (version 1).xlsx";

            foreach (var file in hts_files)
            {
                if (file.Contains(@"\~$"))
                    continue;

                Console.WriteLine(file);

                using (ExcelPackage hts_package = new ExcelPackage(new FileInfo(file)))
                {
                    var htc_sheets = hts_package.Workbook.Worksheets;

                    var firstSheet = htc_sheets.FirstOrDefault();

                    object templateId = firstSheet.Cells["B10"].Value;

                    if (templateId == null || templateId.ToString().Contains("D") == false)
                    {
                        Console.WriteLine("Not a valid hts_tst file. Template id =" + templateId);
                        continue;
                    }


                    object fID = firstSheet.Cells["F5"].Value;
                    if (fID == null)
                    {
                        fID = firstSheet.Cells["E2"].Value;
                    }
                    if (fID == null)
                    {
                        Console.WriteLine("Invalid file - " + file);
                        continue;
                    }
                    string facilityId = fID.ToString();

                    //throw new ApplicationException("Invalid HTS File");

                    ExcelPackage art_pmtct_package = null;
                    ExcelWorksheets art_pmtct_sheets = null;

                    foreach (var f in art_pmtct_files)
                    {
                        if (f.Contains(@"\~$"))
                            continue;

                        art_pmtct_package = new ExcelPackage(new FileInfo(f));
                        art_pmtct_sheets = art_pmtct_package.Workbook.Worksheets;
                        var art_pmtct_sheet = art_pmtct_sheets.FirstOrDefault();
                        string art_pmtct_site_id = art_pmtct_sheet.Cells["F5"].Value.ToString();
                        if (art_pmtct_site_id == facilityId)
                        {
                            break;
                        }
                        art_pmtct_package.Dispose();
                    }
                    if (art_pmtct_package == null || art_pmtct_package.File == null)
                    {
                        continue;
                        //throw new ApplicationException("unable to locate second source");
                    }



                    foreach (var htc_sheet in htc_sheets)
                    {
                        using (ExcelPackage template_package = new ExcelPackage(new FileInfo(template)))
                        {
                            var template_sheet = template_package.Workbook.Worksheets.FirstOrDefault();
                            for (int i = 1; i < mapping.Count(); i++)
                            {
                                string[] z = mapping[i].Split(',');

                                if (!string.IsNullOrEmpty(z[1]))
                                {
                                    if (z[3].Contains("HTS_TST_Didagg"))
                                    {
                                        if (z[2].Contains("*"))
                                        {
                                            string[] rows = z[2].Split('*');
                                            string istCell = ReadaCel(htc_sheet, rows[0]);
                                            string sndCell = ReadaCel(htc_sheet, rows[1]);

                                            template_sheet.Cells[z[0]].Value = istCell.ToInt() + sndCell.ToInt();
                                        }
                                        else
                                        {
                                            template_sheet.Cells[z[1]].Value = ReadaCel(htc_sheet, z[2]);
                                        }
                                    }
                                    else //art_pmtct
                                    {
                                        var art_pmtct_sheet = art_pmtct_sheets[htc_sheet.Name];
                                        if (z[2].Contains("*"))
                                        {
                                            string[] rows = z[2].Split('*');
                                            string istCell = ReadaCel(art_pmtct_sheet, rows[0]);
                                            string sndCell = ReadaCel(art_pmtct_sheet, rows[1]);

                                            template_sheet.Cells[z[0]].Value = istCell.ToInt() + sndCell.ToInt();
                                        }
                                        else
                                        {
                                            template_sheet.Cells[z[1]].Value = ReadaCel(art_pmtct_sheet, z[2]);
                                        }
                                    }
                                }
                            }

                            template_sheet.Cells["B4"].Value = htc_sheet.Cells["F5"].Value != null ? htc_sheet.Cells["F5"].Value.ToString().GetFirstString() : "";
                            template_sheet.Cells["J4"].Value = htc_sheet.Cells["F4"].Value;
                            template_sheet.Cells["E4"].Value = htc_sheet.Cells["F6"].Value;
                            template_sheet.Cells["B3"].Value = htc_sheet.Cells["D6"].Value;
                            template_sheet.Cells["E3"].Value = htc_sheet.Cells["D7"].Value;

                            string filename = "HTS_TST_" + Path.GetFileNameWithoutExtension(file) + "_" + htc_sheet.Name + ".xlsx";
                            template_package.SaveAs(new FileInfo(outputDirectory + filename));
                        }
                    }
                }
            }
        }


        public static void ProcessPMTCTFile()
        {
            string baseLocation = @"C:\Users\cmadubuko\Documents\Mosis\facilities\Apr 2017_Enugu_Final\Apr 2017_Enugu_Final\Enugu SEEDS_Apr 2017\PMTCT\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Imo State _SEED_March 2017\Seeds\IMO PMTCT\";

            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi_Reports_March_2017\Ebonyi_Reports_March_2017\Seeds report\PMTCT\";
            string processedFileDirectory = @"C:\Users\cmadubuko\Documents\Mosis\facilities\Enugu PMTCT Processed\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\IMO PMTCT Processed\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi PMTCT Processed\";

            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.AllDirectories);

            string[] mapping = File.ReadAllText(@"C:\Users\cmadubuko\Documents\Mosis\Mapping PMTCT MSF.csv").Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string template = @"C:\Users\cmadubuko\Documents\Mosis\PMTCT MSF (version 1).xlsx";

            foreach (var file in files)
            {
                if (file.Contains(@"\~$"))
                    continue;

                Console.WriteLine(file);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var pSheets = package.Workbook.Worksheets;
                    foreach (var aSheet in pSheets)
                    {
                        using (ExcelPackage template_package = new ExcelPackage(new FileInfo(template)))
                        {
                            var sheet = template_package.Workbook.Worksheets.FirstOrDefault();
                            for (int i = 1; i < mapping.Count(); i++)
                            {
                                string[] z = mapping[i].Split(',');

                                if (!string.IsNullOrEmpty(z[1]))
                                {
                                    if (z[1].Contains("*"))
                                    {
                                        string[] rows = z[1].Split('*');
                                        string istCell = ReadaCel(aSheet, rows[0]);
                                        string sndCell = ReadaCel(aSheet, rows[1]);

                                        sheet.Cells[z[0]].Value = istCell.ToInt() + sndCell.ToInt();
                                    }
                                    else
                                    {
                                        sheet.Cells[z[0]].Value = ReadaCel(aSheet, z[1]);
                                    }
                                }
                            }

                            sheet.Cells["C5"].Value = aSheet.Cells["F5"].Value.ToString().GetFirstString();
                            sheet.Cells["C6"].Value = aSheet.Cells["F4"].Value;
                            sheet.Cells["E5"].Value = aSheet.Cells["F6"].Value;
                            sheet.Cells["C7"].Value = aSheet.Cells["D6"].Value;
                            sheet.Cells["E7"].Value = aSheet.Cells["D7"].Value;

                            string filename = "PMTCT_" + Path.GetFileNameWithoutExtension(file) + "(" + aSheet.Name + ").xlsx";
                            template_package.SaveAs(new FileInfo(processedFileDirectory + filename));
                        }
                    }
                }
            }
        }



        public static void ProcessFile()
        {
            string baseLocation = @"C:\Users\cmadubuko\Documents\Mosis\facilities\Imo State _SEED_March 2017\Seeds\Imo ART\";

            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Apr 2017_Enugu_Final\Apr 2017_Enugu_Final\Enugu SEEDS_Apr 2017\ART\";
            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi_Reports_March_2017\Ebonyi_Reports_March_2017\Seeds report\ART\";
            string processedFileDirectory = @"C:\Users\cmadubuko\Documents\Mosis\facilities\IMO ART Processed\";

            //"C:\Users\cmadubuko\Documents\Mosis\facilities\Ebonyi ART processed\";

            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.AllDirectories);

            string[] mapping = File.ReadAllText(@"C:\Users\cmadubuko\Documents\Mosis\MAPPING_ART MSF.csv").Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string template = @"C:\Users\cmadubuko\Documents\Mosis\ART MSF.xlsx";

            foreach (var file in files)
            {
                if (file.Contains(@"\~$"))
                    continue;
                Console.WriteLine(file);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var pSheets = package.Workbook.Worksheets;
                    foreach (var aSheet in pSheets)
                    {
                        using (ExcelPackage template_package = new ExcelPackage(new FileInfo(template)))
                        {
                            var sheet = template_package.Workbook.Worksheets.FirstOrDefault();
                            for (int i = 1; i < mapping.Count(); i++)
                            {
                                string[] z = mapping[i].Split(',');

                                if (!string.IsNullOrEmpty(z[4]))
                                {
                                    if (z[4].Contains("*"))
                                    {
                                        string[] rows = z[4].Split('*');
                                        string istLine = ReadaCel(aSheet, rows[0].ToInt(), z[5].ToInt());
                                        string sndLine = ReadaCel(aSheet, rows[1].ToInt(), z[5].ToInt());

                                        sheet.Cells[z[1].ToInt(), z[2].ToInt()].Value = istLine.ToInt() + sndLine.ToInt();

                                        istLine = ReadaCel(aSheet, rows[0].ToInt(), z[6].ToInt());
                                        sndLine = ReadaCel(aSheet, rows[1].ToInt(), z[6].ToInt());

                                        sheet.Cells[z[1].ToInt(), z[3].ToInt()].Value = istLine.ToInt() + sndLine.ToInt();
                                    }
                                    else
                                    {
                                        sheet.Cells[z[1].ToInt(), z[2].ToInt()].Value = ReadaCel(aSheet, z[4].ToInt(), z[5].ToInt());
                                        sheet.Cells[z[1].ToInt(), z[3].ToInt()].Value = ReadaCel(aSheet, z[4].ToInt(), z[6].ToInt());
                                    }
                                }
                            }
                            object previously = sheet.Cells["A2"].Value + " " + aSheet.Cells["F4"].Value;
                            sheet.Cells["A2"].Value = previously;
                            previously = sheet.Cells["A4"].Value + " " + aSheet.Cells["F6"].Value;
                            sheet.Cells["A4"].Value = previously;
                            previously = sheet.Cells["C3"].Value + " " + aSheet.Cells["D6"].Value;
                            sheet.Cells["C3"].Value = previously;
                            previously = sheet.Cells["C4"].Value + " " + aSheet.Cells["D7"].Value;
                            sheet.Cells["C4"].Value = previously;
                            sheet.Cells["B3"].Value = aSheet.Cells["F5"].Value.ToString().GetFirstString();

                            string filename = "ART_" + Path.GetFileNameWithoutExtension(file) + "_(" + aSheet.Name + ").xlsx";
                            template_package.SaveAs(new FileInfo(processedFileDirectory + filename));
                        }
                    }
                }
            }
        }

        private static string ReadaCel(ExcelWorksheet sheet, int row, int column)
        {
            if (row == 0 || column == 0)
                return "";

            return ExcelHelper.ReadCell(sheet, row, column);
        }

        private static string ReadaCel(ExcelWorksheet sheet, string address)
        {
            return ExcelHelper.ReadCell(sheet, address);
        }


        public static void GeneratePeadLineListing()
        {
            string directory = System.Configuration.ConfigurationManager.AppSettings["excelFile"];
            string sql = System.Configuration.ConfigurationManager.AppSettings["peadQuery"];
            SqlCommand cmd = new SqlCommand(sql);
            DataTable dataTable = RetrieveAsDataTable(cmd);
            cmd.Dispose();

            using (ExcelPackage package = new ExcelPackage())
            {
                string currentFacility = "";
                int row = 2;
                ExcelWorksheet workSheet = null;
                foreach (DataRow dr in dataTable.Rows)
                {
                    var facility = dr.Field<string>("Facility");
                    if(facility.Length > 29)
                    {
                        facility = facility.Substring(0, 29);
                    }
                    
                    if (string.IsNullOrEmpty(currentFacility))
                    {
                        package.Workbook.Worksheets.Add(facility);
                        workSheet = package.Workbook.Worksheets[facility];
                        workSheet.Cells["A1"].Value = "IP";
                        workSheet.Cells["B1"].Value = "STATE";
                        workSheet.Cells["C1"].Value = "LGA";
                        workSheet.Cells["D1"].Value = "Facility";
                        workSheet.Cells["E1"].Value = "PatientId";
                        workSheet.Cells["F1"].Value = "HospitalNo";
                        workSheet.Cells["G1"].Value = "Age at start of ART (in_years)";
                        workSheet.Cells["H1"].Value = "Age at start of ART (in_months)";
                        workSheet.Cells["I1"].Value = "Sex";
                        workSheet.Cells["J1"].Value = "ART Start Date";
                        workSheet.Cells["K1"].Value = "CurrentARTStatus";
                        currentFacility = facility;
                    }
                    else if (facility != currentFacility)
                    {
                        package.SaveAs(new FileInfo(directory + "/" + currentFacility + ".xlsx"));
                        package.Workbook.Worksheets.Delete(1);
                        package.Workbook.Worksheets.Add(facility);
                        workSheet = package.Workbook.Worksheets[facility];
                        workSheet.Cells["A1"].Value = "IP";
                        workSheet.Cells["B1"].Value = "STATE";
                        workSheet.Cells["C1"].Value = "LGA";
                        workSheet.Cells["D1"].Value = "Facility";
                        workSheet.Cells["E1"].Value = "PatientId";
                        workSheet.Cells["F1"].Value = "HospitalNo";
                        workSheet.Cells["G1"].Value = "Age at start of ART (in_years)";
                        workSheet.Cells["H1"].Value = "Age at start of ART (in_months)";
                        workSheet.Cells["I1"].Value = "Sex";
                        workSheet.Cells["J1"].Value = "ART Start Date";
                        workSheet.Cells["K1"].Value = "CurrentARTStatus";
                        currentFacility = facility;
                        row = 2;
                    }
                    
                    workSheet.Cells["A" + row].Value = dr.Field<string>("Shortname");
                    workSheet.Cells["B" + row].Value = dr.Field<string>("state_name");
                    workSheet.Cells["C" + row].Value = dr.Field<string>("lga_name");
                    workSheet.Cells["D" + row].Value = dr.Field<string>("Facility");
                    workSheet.Cells["E" + row].Value = dr.Field<string>("PatientId");
                    workSheet.Cells["F" + row].Value = dr.Field<string>("HospitalNo");
                    workSheet.Cells["G" + row].Value = dr.Field<int>("Age_at_start_of_ART_in_years");
                    workSheet.Cells["H" + row].Value = dr.Field<int>("Age_at_start_of_ART_in_months");
                    workSheet.Cells["I" + row].Value = dr.Field<string>("Sex");
                    workSheet.Cells["J" + row].Value = dr.Field<DateTime>("ARTStartDate");
                    workSheet.Cells["K" + row].Value = dr.Field<string>("CurrentARTStatus");
                    row++; 
                }

                //package.Save();
            }
        }


        public static DataTable RetrieveAsDataTable(SqlCommand cmd)
        {
            DataTable ds = new DataTable();
            string sqlConnection = System.Configuration.ConfigurationManager.AppSettings["shield_db"];
            try
            {
                using (var conn = new SqlConnection(sqlConnection))
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd.Connection = conn;
                    da.SelectCommand = cmd;

                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    da.Fill(ds);
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }
    }

}
