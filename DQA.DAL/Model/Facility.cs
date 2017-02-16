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
        public Facility(HealthFacility facility)
        {

            State = facility.lga.state.state_name;
            Lga = facility.lga.lga_name;
            Id = (int)facility.Id;
            SiteName = facility.Name;
            FacilityType = facility.OrganizationType;//Utility.GetFacilityType(Convert.ToInt32(facility.Facility_Type));
            //FacilityLevel = Utility.GetFacilityLevel(Convert.ToInt32(facility.Facility_Level));
            
        }
        public int Id { set; get; }
        public string SiteName { set; get; }
        public string Lga { set; get; }
        public string State { set; get; }
        public string FacilityType { set; get; }
        public string FacilityLevel { set; get; }


    }
}