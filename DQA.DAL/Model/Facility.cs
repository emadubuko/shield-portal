using DQA.DAL.Business;
using DQA.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DQA.ViewModel
{
    public class Facility
    {
        public Facility(dqa_facility facility)
        {
            var state = Utility.GetState(Convert.ToInt32(facility.State.Value));
            if (state != null) State = state.state_name;
            if (facility.LGA != null || facility.LGA != "")
            {
                var lga = Utility.GetLga(Convert.ToInt32(facility.LGA));
                if (lga != null) Lga = lga.lga_name;
            }
            Id = facility.Id;
            SiteName = facility.Site_Name;
            FacilityType = Utility.GetFacilityType(Convert.ToInt32(facility.Facility_Type));
            FacilityLevel = Utility.GetFacilityLevel(Convert.ToInt32(facility.Facility_Level));
            
        }
        public int Id { set; get; }
        public string SiteName { set; get; }
        public string Lga { set; get; }
        public string State { set; get; }
        public string FacilityType { set; get; }
        public string FacilityLevel { set; get; }


    }
}