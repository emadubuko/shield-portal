using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.Models
{
    public class eGsmModels
    {

    }

    public class HTSReport
    {
        public int HTS_TST { get; set; }
        public double HTS_TST_POS { get; set; }
        public string Yield { get; set; }
        public string Num_Find_Positive { get; set; }
    }

    public class SDPAggregate
    {
        public string SDP_Name { get; set; }
        public double yield { get; set; } 
    }
}