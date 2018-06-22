using System;
using System.Collections.Generic;

namespace ShieldPortal.ViewModel
{
    public class RadetReportModel
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string UploadedBy { get; set; }
        public int dqa_year { get; set; }
        public string dqa_quarter { get; set; }
        public DateTime DateUploaded { get; set; }
        public string Facility { get; internal set; }
    }
     

    public class ExportData
    {
        public string IPShortName { get; set; }
        public string Facility { get; set; }
        public string PatientId { get; set; }
        public string HospitalNo { get; set; }
        public string Sex { get; set; }
        public int AgeInMonths { get; set; }
        public int AgeInYears { get; set; }
        public DateTime? ARTStartDate { get; set; }
        public DateTime? LastPickupDate { get; set; }
        public int MonthsOfARVRefill { get; set; }
        public string RegimenLineAtARTStart { get; set; }
        public string RegimenAtStartOfART { get; set; }
        public string CurrentRegimenLine { get; set; }
        public string CurrentARTRegimen { get; set; }
        public string PregnancyStatus { get; set; }
        public string CurrentViralLoad { get; set; }
        public DateTime? DateOfCurrentViralLoad { get; set; }
        public string ViralLoadIndication { get; set; }
        public string CurrentARTStatus { get; set; }
        public bool RandomlySelect { get; set; }
        public string RadetPeriod { get; set; }
        public string LGA { get; set; }
        public string State { get; set; }

        public string CurrentAge
        {
            get
            {
                double days = DateTime.Now.Subtract(ARTStartDate.Value).TotalDays;
                if (AgeInYears != 0)
                {
                    return AgeInYears + Math.Round(days / 365, 0) + " years";
                }
                else
                    return AgeInMonths + Math.Round(days / 30, 0) + " months";
            }
        }
    }

    public class RandomizerModel
    {
        public IEnumerable<string> IP { get; set; }
        public IEnumerable<string> Facility { get; set; }
        public IEnumerable<CommonUtil.Entities.LGA> LGA { get; set; }
        public IEnumerable<string> RadetPeriod { get; set; }
        public bool AllowCriteria { get; set; }
    }

    
    public class IPLGAFacility
    {
        public string IP { get; set; }
        public string FacilityName { get; set; }
        public CommonUtil.Entities.LGA LGA { get; set; }
        public string RadetPeriod { get; set; }
    }     

    public class RandomizerDropDownModel
    {
        public List<IPLGAFacility> IPLocation { get; set; }
        public bool AllowCriteria { get; set; }
    }

}
 