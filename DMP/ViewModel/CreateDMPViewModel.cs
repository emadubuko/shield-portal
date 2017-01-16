using DAL.Entities;
using System;
using System.Collections.Generic;

namespace DMP.ViewModel
{
    public class CreateDMPViewModel : AutomaticViewModel<DAL.Entities.DMP>
    {       
        public ICollection<Organizations> OrganizationList { get; set; }

        public Dictionary<string, string> ToolTip = new Dictionary<string, string>
        {
            { "DMPTitle", "Provide a title for the DMP" },
        };
    }
            
}