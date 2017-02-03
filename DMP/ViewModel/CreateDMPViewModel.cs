using CommonUtil.Entities;
using System.Collections.Generic;

namespace ShieldPortal.ViewModel
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