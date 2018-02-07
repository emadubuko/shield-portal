using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQI.DAL.Entity
{
    public class metatdata
    {
        public virtual string QI_Approach { get; set; }
        public virtual string DataCollectionMethod { get; set; }
        public virtual string WhatIstheProblem { get; set; }
        public virtual string HowDoYouKnowThisIsAProblem { get; set; }
        public virtual string HowWillYouKnowWhenTheProblemIsResolved { get; set; }
        public virtual List<string> WhyDoesTheProblemOccur { get; set; }
        public virtual string WhichQIApproachDidYouUseToAnalyzeTheProblem { get; set; } 
        public virtual List<string> PossibleInterventions {get;set;}
        public virtual string QIApproachForIntervention { get; set; }

        public virtual string MostViableIntervention { get; set; }
        public virtual List<string> ProcessTracking { get; set; }

        public virtual string HowOftenWillIndicatorBeMeasured { get; set; }

        public virtual string HowWillIndicatorBeEvaluated { get; set; }

        public virtual WeeklyResultData WeeklyResultData { get; set; }
    }

}
  