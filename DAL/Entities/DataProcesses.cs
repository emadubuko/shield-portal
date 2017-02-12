using System.Collections.Generic;

namespace DAL.Entities
{
    public class DataProcesses
    {
        public virtual List<DataCollection> DataCollection { get; set; }
        public virtual Report Reports { get; set; }
    }
}
