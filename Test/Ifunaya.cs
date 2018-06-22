using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
  public  class Ifunaya
    {
        static void DoMerge(string[] files, string IP, ref int row)
        {
            
            string baseLocation = @"C:\MGIC\Document\SHIELD\OVC custom indicators report SAPR";

            foreach (var file in files)
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var sheets = package.Workbook.Worksheets;
                    foreach (var sheet in sheets)
                    {
                        string lga = sheet.Name.Split(new string[] { "LGA" }, StringSplitOptions.None)[0];


                        int col = 8;
                        using (ExcelPackage output = new ExcelPackage(new FileInfo(baseLocation + "\\USG Quarterly OVC Reporting Template_FINAL_Guide.xlsx")))
                        {
                            var Guidesheet = output.Workbook.Worksheets.LastOrDefault();

                            Guidesheet.Cells[row, 1].Value = IP;
                            Guidesheet.Cells[row, 2].Value = "";
                            Guidesheet.Cells[row, 3].Value = lga;

                            for (int original_sheet_col = 5; original_sheet_col <= 65; original_sheet_col++)
                            {
                                var cell = sheet.Cells[original_sheet_col, 19];

                                Guidesheet.Cells[row, col].Value = cell.Value;
                                col++;
                            }
                            output.Save();
                        }

                        row++;
                    }
                }
            }
        }
      public static void MergeFiles()
        {
            int row = 4;
            string baseLocation = @"C:\MGIC\Document\SHIELD\OVC custom indicators report SAPR";

            string[] apin_files = Directory.GetFiles(baseLocation + "\\apin", "*.xls*", SearchOption.AllDirectories);
            string[] ccfn_files = Directory.GetFiles(baseLocation + "\\ccfn", "*.xls*", SearchOption.AllDirectories);
            string[] cihp_files = Directory.GetFiles(baseLocation + "\\chip", "*.xls*", SearchOption.AllDirectories);
            string[] ihvn_files = Directory.GetFiles(baseLocation + "\\ihvn", "*.xls*", SearchOption.AllDirectories);
            
            DoMerge(apin_files, "APIN", ref row);
            DoMerge(ccfn_files, "CCFN", ref row);
            DoMerge(cihp_files, "CIHP", ref row);
            DoMerge(ihvn_files, "IHVN", ref row);
             
        }

    }
}
