using DAL.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMP.ViewModel
{
    public class ProfileViewModel : AutomaticViewModel<Profile>
    {
        public IList<Organizations> Organization { get; set; }

        public IQueryable<IdentityRole> Role { get; set; }
    }
}