using System.Collections.Generic;

namespace ShieldPortal.ViewModel.DMP
{
    public class StaffStatusByRoles
    {
        public Dictionary<string, int> RoleCount { get; set; } 
        public string IP { get; set; }
        public Dictionary<string, int> WorkStation { get; set; }
    }
}