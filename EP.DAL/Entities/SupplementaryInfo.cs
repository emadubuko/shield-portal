using System;
using System.Collections.Generic;

namespace EP.DAL.Entities
{
    public class SupplementaryInfo
    { 
        public virtual List<info> Info { get; set; }
    } 

    public class info
    {
        public int id { get; set; }
        public int evaluationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PosterName { get; set; }
        public string PostedDate { get; set; }
    }
}
