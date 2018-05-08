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
        public IList<string> ImplementingPartner { get; set; }
    }

    public class IPUploadReport
    {
        public int Id { get; set; }
        public string IPName { get; set; }
        public string ReportPeriod { get; set; }
    }
}
