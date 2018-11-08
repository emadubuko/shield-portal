using System;
using System.Collections.Generic;

namespace MPM.DAL
{

    public class MPMFacilityListing
    {
        public string IP { get; set; }
        public string Facility { get; set; }
        public string DATIMCode { get; set; }
        public bool HTS { get; set; }
        public bool PMTCT { get; set; }
        public bool ART { get; set; }
        public bool TB { get; set; }
    }


    public class UploadViewModel
    {
        public Dictionary<string, List<bool>> IPReports { get; set; }

        //public string SelectedYear { get; set; }

        public List<string> Year = new List<string>
        {
           DateTime.Now.Year.ToString(),
        };
        public Dictionary<string, int> IndexPeriods { get; set; }
        public string ImplementingPartner { get; set; }
    }

    public class IPUploadReport
    {
        public int Id { get; set; }
        public string IPName { get; set; }
        public string ReportPeriod { get; set; }
        public string ReportingLevelValue { get; set; }
        public string FilePath { get; set; }
    }


    public class MPMDataSearchModel
    {
        public List<string> IPs { get; set; }
        public List<string> lga_codes { get; set; }
        public List<string> state_codes { get; set; }
        public List<string> facilities { get; set; }
        public string ReportPeriod { get; set; }
        public string PopulationGroup { get; set; }
        public string Sex { get; set; }
        public string Agegroup { get; set; }

    }


}
