using System.Collections.Generic;

namespace DMP.BWR.ViewModel
{
    public class GPSDataModel
    {
        public string location { get; set; }
    }

    public class GoogleAPIResponse
    {
        public List<GoogleAPIResults> results { get; set; }
        public string status { get; set; }
    }

    public class GoogleAPIResults
    {
        public string formatted_address { get; set; }
        public GoogleGeometry geometry { get; set; }
        public string place_id { get; set; }
    }

    public class GoogleGeometry
    {
        public GoogleLocation location { get; set; }
    }
    public class GoogleLocation
    {
        public string lat { get; set; }
        public string lng { get; set; }
    }

    public class Cordinate
    {
        public string responseCode { get; set; }
        public string identifier { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}