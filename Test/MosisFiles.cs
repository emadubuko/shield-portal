using CommonUtil.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class MosisFiles
    {
        public static void ProcessFile()
        {
            string baseLocation = @"C:\Users\cmadubuko\Documents\Mosis\facilities\";
            string processedFileDirectory = @"C:\Users\cmadubuko\Documents\Mosis\Processed\";

            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.AllDirectories);

            string[] mapping = File.ReadAllText(@"C:\Users\cmadubuko\Documents\Mosis\MAPPING_MSFs.csv").Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string template = @"C:\Users\cmadubuko\Documents\Mosis\ART MSF.xlsx";

            foreach (var file in files)
            {
                Console.WriteLine(file);
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var pSheets = package.Workbook.Worksheets;
                    foreach (var aSheet in pSheets)
                    {
                        using (ExcelPackage template_package = new ExcelPackage(new FileInfo(template)))
                        {
                            var sheet = template_package.Workbook.Worksheets.FirstOrDefault();
                            foreach (var line in mapping)
                            {
                                List<int> z = new List<int>();
                                line.Split(',').ToList().ForEach(x =>
                                {
                                    int i = 0;
                                    int.TryParse(x, out i);
                                    z.Add(i);
                                });

                                if (z[5] != 0)
                                {
                                    sheet.Cells[z[1], z[2]].Value = ReadaCel(aSheet, z[5], z[6]);
                                    sheet.Cells[z[1], z[3]].Value = ReadaCel(aSheet, z[5], z[7]);
                                    sheet.Cells[z[1], z[4]].Value = ReadaCel(aSheet, z[5], z[8]);
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


                            string filename = file.Split(new string[] { @"\" }, StringSplitOptions.None)[6].Split(new string[] { ".xlsx" }, StringSplitOptions.RemoveEmptyEntries)[0] + "_" + aSheet.Name + ".xlsx";
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
    }
}
