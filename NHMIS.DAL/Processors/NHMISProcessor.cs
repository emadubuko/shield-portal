using CommonUtil.Utilities;
using NHMIS.DAL.BizModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMIS.DAL.Processors
{
   public class NHMISProcessor
    {
        public void processFile(Stream pivot, Stream nhs)
        {
            List<NHMIS_Pivot_Model> pivot_model = new List<NHMIS_Pivot_Model>();
            List<NHMIS_Excel_model> nhmis_model = new List<NHMIS_Excel_model>();

            using (ExcelPackage package = new ExcelPackage(pivot))
            {
                var sheet = package.Workbook.Worksheets.FirstOrDefault();
                int row = 2;
                while (true)
                {

                    NHMIS_Pivot_Model model = new NHMIS_Pivot_Model
                    {
                        Site = sheet.Cells[row, 1].Value.ToString(),
                        Sex = "Female",
                        //Indicator = sheet.Cells[row, 2].Value.ToString(),
                        _10_14yrs = sheet.Cells[row, 3].Value.ToString().ToInt(),
                         _15_19yrs = sheet.Cells[row, 4].Value.ToString().ToInt(),
                    };

                }
                
            }
        }
    }
}
