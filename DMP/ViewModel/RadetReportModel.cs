using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class RadetReportModel
    {
        public  int Id { get; set; }
        public  string  IP { get; set; }
        public  string UploadedBy { get; set; }
        public  int dqa_year { get; set; }
        public  string dqa_quarter { get; set; }
        public  IList<RadetListing> Uploads { get; set; }
        public  DateTime DateUploaded { get; set; }
    }
    public class RadetListing
    {
        public  int Id { get; set; }
        public  string PatientId { get; set; }
        public  string HospitalNo { get; set; }
        public  string Sex { get; set; }
        public  string Age_at_start_of_ART_in_years { get; set; }
        public  string Age_at_start_of_ART_in_months { get; set; }
        public  string ARTStartDate { get; set; }
        public  string LastPickupDate { get; set; }
        public  string MonthsOfARVRefill { get; set; }
        public  string RegimenLineAtARTStart { get; set; }
        public  string RegimenAtStartOfART { get; set; }
        public  string CurrentRegimenLine { get; set; }
        public  string CurrentARTRegimen { get; set; }
        public  string Pregnancy_Status { get; set; }
        public  string Current_Viral_Load { get; set; }
        public  string Date_of_Current_Viral_Load { get; set; }
        public  string Viral_Load_Indication { get; set; }
        public  string CurrentARTStatus { get; set; }

        public  bool SelectedForDQA { get; set; }
        public  string RadetYear { get; set; } 
    }
}
 