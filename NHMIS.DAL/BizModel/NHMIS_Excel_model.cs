using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMIS.DAL.BizModel
{
    public class NHMIS_Excel_model
    {
        public string Site { get; set; }
        public string Sex { get; set; }
        public string indicator { get; set; }
        public int _1yrs { get; set; }
        public int _1_4yrs { get; set; }
        public int _5_9yrs { get; set; }
        public int _10_14yrs { get; set; }
        public int _15_19yrs { get; set; }
        public int _20_24yrs { get; set; }
        public int _25_49yrs { get; set; }
        public int _50yrs { get; set; }

    }

    public class NHMIS_Pivot_Model
    {
        public string Site { get; set; }
        public string Indicator { get; set; }
        public string Sex { get; set; }
        public int _1yrs { get; set; }
        public int _1_4yrs { get; set; }
        public int _5_9yrs { get; set; }
        public int _10_14yrs { get; set; }
        public int _15_19yrs { get; set; }
        public int _20_24yrs { get; set; }
        public int _25_49yrs { get; set; }
        public int _50yrs { get; set; }
    }
}
