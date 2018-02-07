using CommonUtil.DAO;
using CommonUtil.Entities;
using DQI.DAL.DAO;
using DQI.DAL.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DQI.DAL.Services
{
    public class QIEngine
    {
        public DQIModel GetQISites(string reportPeriod, string ip = "")
        {
            var data = SQLUtility.GetSitesForDQI(reportPeriod, ip);
            var facilities = new HealthFacilityDAO().RetrieveAll();

            var dqiModel = new DQIModel();
            var sites = new List<DQISites>();

            foreach (DataRow row in data.Rows)
            {
                sites.Add(new DQISites
                {
                    IP = row[0].ToString(),
                    Facilty = facilities.FirstOrDefault(x => x.Id == Convert.ToInt32(row[2])),
                    Indicator = row[3].ToString(),
                    indicatorConcurrence = Convert.ToDouble(row[4]).ToString("N2"),
                    IndicatorDeviation = Convert.ToDouble(row[5]).ToString("N2"),
                    DATIM = Convert.ToDouble(row[6].ToString()),
                    Validated = Convert.ToDouble(row[7].ToString())
                });
            }

            dqiModel.DQISites = sites;

            var iPGrouping = sites.GroupBy(x => x.IP);
            var iplevel = new List<IPLevelDQI>();

            foreach (var group in iPGrouping)
            {
                var facility_indicator_grouping = group.ToList().GroupBy(x => x.Indicator);
                string indicator = "";
                int previousCount = 0;
                double concurrence = 0;
                foreach (var f in facility_indicator_grouping)
                {
                    if (f.ToList().Count() > previousCount)
                    {
                        previousCount = f.ToList().Count();
                        indicator = f.Key;
                        concurrence = 100 * f.ToList().Sum(x => x.Validated) / f.ToList().Sum(x => x.DATIM);
                    }
                }


                iplevel.Add(new IPLevelDQI
                {
                    IP = group.Key,
                    NoOfSitesAffected = previousCount,
                    TotalSites = group.ToList().Count(),
                    AverageConcurrence = concurrence.ToString("N2"),
                    WorstPerformingIndicator = indicator,
                });
            }

            dqiModel.IPLevel = iplevel;

            return dqiModel;
        }

        public bool ProcessUpload(Stream uploadedFile, string v, Profile loggedinProfile, out string result)
        {
            throw new NotImplementedException();
        }

        public void PopulateTool(IPLevelDQI data, string directory, string fileName, string template)
        { 

            using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
            {
                var sheet = package.Workbook.Worksheets["IP Dashboard"];
                sheet.Cells["C2"].Value = data.IP;
                sheet.Cells["C4"].Value = data.WorstPerformingIndicator;
                sheet.Cells["C6"].Value = data.NoOfSitesAffected;
                sheet.Cells["G3"].Value = data.AverageConcurrence + "%";

                package.SaveAs(new FileInfo(directory + "/" + fileName));
            }
        }
    }
}
