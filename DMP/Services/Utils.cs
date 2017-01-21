using DAL.DAO;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMP.Services
{
    public class Utils
    {
        public Profile GetloggedInProfile()
        {
            ProfileDAO dao = new ProfileDAO();
            Profile profile = null;
            var user = HttpContext.Current.User;
            if (user != null && user.Identity != null && !string.IsNullOrEmpty(user.Identity.Name))
            {
                profile = dao.GetProfileByUsername(user.Identity.Name);
            }
            else
            {
                //return dummy for now 
                Guid pGuid = new Guid("D2ED8EA3-A335-4718-914D-A6F301671679");
                profile = new ProfileDAO().Retrieve(pGuid);
            }

            return profile;
        }
    }
}