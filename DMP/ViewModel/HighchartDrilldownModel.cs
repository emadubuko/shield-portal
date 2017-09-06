using System.Collections.Generic;

namespace ShieldPortal.ViewModel
{
    public class HighchartDrilldownModel
    {
        public Container container { get; set; }
    }

    public class Chart
    {
        public string type { get; set; }
    }

    public class Title
    {
        public string text { get; set; }
    }

    public class Subtitle
    {
        public string text { get; set; }
    }

    public class XAxis
    {
        public string type { get; set; }
    }

    public class YAxis
    {
        public Title title { get; set; }
    }

    public class Legend
    {
        public bool enabled { get; set; }
    }

    public class DataLabels
    {
        public bool enabled { get; set; }
        public string format { get; set; }
    }

    public class PlotSeries
    {
        public int borderWidth { get; set; }
        public DataLabels dataLabels { get; set; }
    }

    public class PlotOptions
    {
        public PlotSeries series { get; set; }
    }

    public class Tooltip
    {
        public string headerFormat { get; set; }
        public string pointFormat { get; set; }
    }

    public class Datum
    {
        public string name { get; set; }
        public double y { get; set; }
        public string drilldown { get; set; }
    }

    public class ParentData
    {
        public string name { get; set; }
        public bool colorByPoint { get; set; }
        public List<Datum> data { get; set; }
    }

    public class ChildData
    {
        public string name { get; set; }
        public string id { get; set; }
        public List<List<object>> data { get; set; }
    }

    public class Drilldown
    {
        public List<ChildData> series { get; set; }
    }

    public class Container
    {
        public Chart chart { get; set; }
        public Title title { get; set; }
        public Subtitle subtitle { get; set; }
        public XAxis xAxis { get; set; }
        public YAxis yAxis { get; set; }
        public Legend legend { get; set; }
        public PlotOptions plotOptions { get; set; }
        public Tooltip tooltip { get; set; }
        public List<ParentData> series { get; set; }
        public Drilldown drilldown { get; set; }
    }


}