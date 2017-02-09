using BWReport.DAL.Entities;
using System;
using System.Collections.Generic;

namespace ShieldPortal.ViewModel.BWR
{
    public class ReportViewModel
    {
        public IList<LGALevelAchievementPerTarget> LGAReports { get; set; }

        public IList<Facility_Community_Postivity> FacilityPositivty { get; set; }
        public IList<Facility_Community_Postivity> CommunityPositivty { get; set; }

        public string SelectedYear { get; set; }

        public List<string> Year = new List<string>
        {
            (DateTime.Now.Year -1).ToString(),
            DateTime.Now.Year.ToString(),
            (DateTime.Now.Year + 1).ToString()
        };
 
        public IList<string> ImplementingPartner { get; set; }
    }


    public class BiWeeklyReportUploadViewModel
    {
        public Dictionary<string, List<bool>> IPReports { get; set; }

        public string SelectedYear { get; set; }

        public List<string> Year = new List<string>
        {
            (DateTime.Now.Year -1).ToString(),
            DateTime.Now.Year.ToString(),
            (DateTime.Now.Year + 1).ToString()
        };
        public Dictionary<string, int> IndexPeriods { get; set; }
        public IList<string> ImplementingPartner { get; set; }
    }
}