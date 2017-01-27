using BWReport.DAL.Entities;
using System;
using System.Collections.Generic;

namespace DMP.ViewModel.BWR
{
    public class ReportViewModel
    {
        public IList<LGALevelAchievementPerTarget> LGAReports { get; set; }

        public IList<Facility_Community_Postivity> FacilityPositivty { get; set; }
        public IList<Facility_Community_Postivity> CommunityPositivty { get; set; }

        public IList<string> ImplementingPartner { get; set; }

        public Dictionary<string, int> IndexPeriods = new Dictionary<string, int>
        {
           {"1-15 Oct", 12}, {"16-31 Oct",15 },  {"1-15 Nov",18 },   {"16-30 Nov",21 },
           { "1-15 Dec",24 }, {"16 -31 Dec",27 }, {"1-15 Jan",30}, {"16-31 Jan",33 }, { "1-15 Feb",36},
           { "16 -29 Feb",39}, {"1-15 Mar",42}, {"16-31 Mar",45 }, { "1-15 Apr",48}, {"16-30 Apr",51 },
           {"1-15 May",54}, { "16-31 May",57}, {"1-15 Jun",60}, { "16-30 Jun",63}, {"1-15 Jul",66},
           {"16-31 Jul",69 }, {"1-15 Aug",72 }, {"16-31 Aug",75 }, {"1-15 Sep",78}, { "16-30 Sep",81}
        };

        public  List<string> Year = new List<string>
        {
            (DateTime.Now.Year -1).ToString(),
            DateTime.Now.Year.ToString(),
            (DateTime.Now.Year + 1).ToString()
        };
    }
}