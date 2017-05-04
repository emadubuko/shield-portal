using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Test
{
    public class AfenetUtil
    {
        public static void ReadFile()
        {
            List<AfenetReport> report = new List<AfenetReport>();
            using (ExcelPackage package = new ExcelPackage(new FileInfo(@"C:\Users\cmadubuko\Google Drive\MGIC\Documents\DQA\AFENET REVISED FINAL_DQA TECHNICAL REPORT.xlsx")))
            {
                var sheet = package.Workbook.Worksheets.FirstOrDefault();

                int row = 5;
                while (true)
                {
                    string cellContent = ExcelHelper.ReadCell(sheet, row, 1);
                    if (string.IsNullOrEmpty(cellContent))
                    {
                        break;
                    }

                    var rpt = new AfenetReport
                    {
                        State = ExcelHelper.ReadCell(sheet, row, 1),
                        Facility = ExcelHelper.ReadCell(sheet, row, 3),
                        IP = ExcelHelper.ReadCell(sheet, row, 4),
                        Services = ExcelHelper.ReadCell(sheet, row, 7),
                        HTC_TST = new IndicatorScore(),
                        HTC_TST_POS = new IndicatorScore(),
                        PMTCT_ARV = new IndicatorScore(),
                        PMTCT_STAT = new IndicatorScore(),
                        PMTCT_STAT_POS = new IndicatorScore(),
                        TX_CURR = new IndicatorScore(),
                        TX_NEW = new IndicatorScore(),
                    };
                     
                    rpt.HTC_TST.SAPR16 = ExcelHelper.ReadCell(sheet, row, 8).ToInt();
                    rpt.HTC_TST.Validated = ExcelHelper.ReadCell(sheet, row, 9).ToInt();
                    rpt.HTC_TST.Concurrence = 100 * ExcelHelper.ReadCell(sheet, row, 10).ToDecimal();

                    rpt.HTC_TST_POS.SAPR16 = ExcelHelper.ReadCell(sheet, row, 11).ToInt();
                    rpt.HTC_TST_POS.Validated = ExcelHelper.ReadCell(sheet, row, 12).ToInt();
                    rpt.HTC_TST_POS.Concurrence = 100 *  ExcelHelper.ReadCell(sheet, row, 13).ToDecimal();

                    rpt.PMTCT_STAT.SAPR16 = ExcelHelper.ReadCell(sheet, row, 14).ToInt();
                    rpt.PMTCT_STAT.Validated = ExcelHelper.ReadCell(sheet, row, 15).ToInt();
                    rpt.PMTCT_STAT.Concurrence = 100 * ExcelHelper.ReadCell(sheet, row, 16).ToDecimal();

                    rpt.PMTCT_STAT_POS.SAPR16 = ExcelHelper.ReadCell(sheet, row, 17).ToInt();
                    rpt.PMTCT_STAT_POS.Validated = ExcelHelper.ReadCell(sheet, row, 18).ToInt();
                    rpt.PMTCT_STAT_POS.Concurrence = 100 * ExcelHelper.ReadCell(sheet, row, 19).ToDecimal();

                    rpt.PMTCT_ARV.SAPR16 = ExcelHelper.ReadCell(sheet, row, 20).ToInt();
                    rpt.PMTCT_ARV.Validated = ExcelHelper.ReadCell(sheet, row, 21).ToInt();
                    rpt.PMTCT_ARV.Concurrence = 100 * ExcelHelper.ReadCell(sheet, row, 22).ToDecimal();

                    rpt.TX_NEW.SAPR16 = ExcelHelper.ReadCell(sheet, row, 23).ToInt();
                    rpt.TX_NEW.Validated = ExcelHelper.ReadCell(sheet, row, 24).ToInt();
                    rpt.TX_NEW.Concurrence = 100 * ExcelHelper.ReadCell(sheet, row, 25).ToDecimal();

                    rpt.TX_CURR.SAPR16 = ExcelHelper.ReadCell(sheet, row, 26).ToInt();
                    rpt.TX_CURR.Validated = ExcelHelper.ReadCell(sheet, row, 27).ToInt();
                    rpt.TX_CURR.Concurrence = 100 * ExcelHelper.ReadCell(sheet, row, 28).ToDecimal();


                    report.Add(rpt);
                    row++;
                }

                var dao = new AfenetReportDAO();
                foreach(var item in report)
                {
                    dao.Save(item);
                }
                dao.CommitChanges();
            }
        }
    }
}
