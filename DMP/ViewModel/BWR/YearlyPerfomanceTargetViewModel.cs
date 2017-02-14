using BWReport.DAL.Entities;
using CommonUtil.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel.BWR
{
    public class YearlyPerfomanceTargetViewModel : AutomaticViewModel<YearlyPerformanceTarget>
    {
        public IList<YearlyPerformanceTarget> Targets { get; set; }
        public IList<HealthFacility> Facilities { get; set; }

        public IList<string> Year = new List<string>
        {
            (DateTime.Now.Year - 1).ToString(),
            DateTime.Now.Year.ToString(),
            (DateTime.Now.Year + 1).ToString()
        };
     }
}