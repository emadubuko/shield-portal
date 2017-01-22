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
            if (HttpContext.Current.Session[".:LoggedInProfile:."] != null)
            {
                return HttpContext.Current.Session[".:LoggedInProfile:."] as Profile;
            }

            ProfileDAO dao = new ProfileDAO();
            Profile profile = null;
            var user = HttpContext.Current.User;
            if (user != null && user.Identity != null && !string.IsNullOrEmpty(user.Identity.Name))
            {
                profile = dao.GetProfileByUsername(user.Identity.Name);
            }
            if (profile == null)
            {
                throw new ApplicationException("Profile info not found");
            }
            //else
            //{
            //    //return dummy for now 
            //    Guid pGuid = new Guid("D2ED8EA3-A335-4718-914D-A6F301671679");
            //    profile = new ProfileDAO().Retrieve(pGuid);
            //}

            return profile;
        }

        public static string LoggedinProfileName
        {
            get
            {
                Profile profile = null;
                if (HttpContext.Current.Session[".:LoggedInProfile:."] != null)
                {
                    profile = HttpContext.Current.Session[".:LoggedInProfile:."] as Profile;
                }

                ProfileDAO dao = new ProfileDAO(); 
                var user = HttpContext.Current.User;
                if (user != null && user.Identity != null && !string.IsNullOrEmpty(user.Identity.Name))
                {
                    profile = dao.GetProfileByUsername(user.Identity.Name);
                    return profile.FullName;
                }
                else
                {
                    return "";
                }               
            }
        }
        public static Guid LoggedinProfileID
        {
            get
            {
                Profile profile = null;
                if (HttpContext.Current.Session[".:LoggedInProfile:."] != null)
                {
                    profile = HttpContext.Current.Session[".:LoggedInProfile:."] as Profile;
                }

                ProfileDAO dao = new ProfileDAO();
                var user = HttpContext.Current.User;
                if (user != null && user.Identity != null && !string.IsNullOrEmpty(user.Identity.Name))
                {
                    profile = dao.GetProfileByUsername(user.Identity.Name);
                    return profile.Id;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }
    }
}