using System;

namespace ShieldPortal.ViewModel
{
    public class RadetReportModel
    {
        public  int Id { get; set; }
        public  string  IP { get; set; }
        public  string UploadedBy { get; set; }
        public  int dqa_year { get; set; }
        public  string dqa_quarter { get; set; } 
        public  DateTime DateUploaded { get; set; }
        public string Facility { get; internal set; }
    }
     

    public class RadetReportModel2
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string UploadedBy { get; set; } 
        public string RadetPeriod { get; set; } 
        public DateTime DateUploaded { get; set; }
        public string Facility { get; internal set; }
    }
     
}
 