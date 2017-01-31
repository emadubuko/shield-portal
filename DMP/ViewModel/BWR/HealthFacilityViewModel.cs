using BWReport.DAL.Entities;
using CommonUtil.Entities;
using CommonUtil.Enums;
using System;
using System.Collections.Generic;

namespace DMP.ViewModel.BWR
{
    public class HealthFacilityViewModel : AutomaticViewModel<HealthFacility>
    {
        private List<string> _organizationType;
        public List<string> OrganizationType
        {
            get
            {
                if (_organizationType != null)
                    return _organizationType;

                _organizationType = new List<string>();
                foreach (var name in Enum.GetNames(typeof(OrganizationType)))
                {
                    _organizationType.Add(CommonUtil.Utilities.Utilities.PasCaseConversion(name));
                }
                return _organizationType;
            }
        }

        public List<Organizations> Organizations { get; set; }
 
        public List<LGA> LGA { get; set; }
    }
}