using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class TrackerViewModel
    {
        public List<GanttChartData> data { get; set; }
        public string DocumentTitle { get; set; }
        //public DMPDocument document { get; set; }
    }

    public class Labels
    {
        public const string trainingLabelClass = "ganttRed";
        public const string dataVerificationLabelClass = "ganttBlue";
        public const string dataCollectionLabelClass = "ganttOrange";
        public const string reportLabelClass = "ganttGreen";

        //public const string trainingLabelName = "T"; //"Training";
        //public const string dataVerificationLabelName = "DV"; //"Data Verification";
        //public const string dataCollectionLabelName = "DC"; //"Data Collection";
        //public const string reportLabelName = "RPT"; // "Report";
    }

    public class GanttChartData
    {
        public string name { get; set; }
        public  string desc { get; set; }
        public List<ChartValues> values { get; set; }
    }

    public class ChartValues
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string label { get; set; }
        public string customClass { get; set; }
        public string dataObj { get; set; }
    }

    
}