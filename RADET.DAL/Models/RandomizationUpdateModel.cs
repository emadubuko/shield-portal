using System;
using System.Collections.Generic;

namespace RADET.DAL.Models
{
    public class RandomizationUpdateModel
    {
        public int Id { get; set; }
        public string CurrentARTStatus { get; set; }
        public bool RandomlySelect { get; set; }
        public string IP { get; set; }
        public string FacilityName { get; set; }
        public int MetadataId { get; set; }
    }

    public class RadetMetaDataSearchModel
    {
        public List<string> IPs { get; set; }
        public List<string> lga_codes { get; set; }
        public List<string> state_codes { get; set; }
        public List<string> facilities { get; set; }
        public string RadetPeriod { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }

        //for single selection
        public int MetaDataId { get; set; }
    }

    public class RadetReportModel2
    {
        public int Id { get; set; }
        public int  DT_RowId { get; set; }
        public string IP { get; set; }
        public string UploadedBy { get; set; }
        public string RadetPeriod { get; set; }
        public DateTime DateUploaded { get; set; }
        public string Facility { get; set; }
        public CommonUtil.Entities.LGA LGA { get; set; }
        public string LGA_code { get; set; }
        public string FirstColumn { get; set; }
        public string LastColumn { get; set; }
    }
}
