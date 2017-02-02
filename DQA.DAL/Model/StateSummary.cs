using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DQA.DAL.Model
{
    public class StateSummary
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public int Percentage { set; get; }
        public int Submitted { set; get; }
        public int Pending { set; get; }
    }
}