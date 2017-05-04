using CommonUtil.DAO;
using CommonUtil.Entities;
using DAL.DAO;
using DAL.Entities;
using ShieldPortal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.Services
{
    public class Utils
    {

        public static IQueryable<IdentityRole> RetrieveRoles()
        {
            var context = new ApplicationDbContext();

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var roles = RoleManager.Roles;

            return roles;
        }

        public static int AddUserToRole(string userId, string rolename)
        {
            var context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            UserManager.AddToRole(userId, rolename);
            return context.SaveChanges();
        }

        public static int UpdateUserToRole(string username, string currentRole, string newRolename)
        { 
            var context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

           var user = UserManager.FindByName(username);
            if (!string.IsNullOrEmpty(currentRole))
            {
                UserManager.RemoveFromRole(user.Id, currentRole);
            }
            if (!string.IsNullOrEmpty(newRolename))
            {
                UserManager.AddToRole(user.Id, newRolename);
            }            
            return context.SaveChanges();
        }


        public Profile GetloggedInProfile()
        {
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[".:LoggedInProfile:."] != null)
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
            return profile;
        }


        public static string DisplayBadge
        {
            get
            {
                Profile profile = null;
                if (HttpContext.Current.Session[".:LoggedInProfile:."] != null)
                {
                    profile = HttpContext.Current.Session[".:LoggedInProfile:."] as Profile;
                }
                else
                {
                    ProfileDAO dao = new ProfileDAO();
                    var user = HttpContext.Current.User;
                    if (user != null && user.Identity != null && !string.IsNullOrEmpty(user.Identity.Name))
                    {
                        profile = dao.GetProfileByUsername(user.Identity.Name);
                    }
                }
                if (profile != null && profile.RoleName == "ip")
                {
                    return string.Format("{0} - Implementing partner ({1})", profile.FullName, profile.Organization.ShortName);
                }
                else
                {
                    return string.Format("{0}", profile.FullName);
                }
                 
            }
        }

        public static string LoggedinProfileName
        {
            get
            {
                Profile profile = null;
                if (HttpContext.Current.Session[".:LoggedInProfile:."] != null)
                {
                    profile = HttpContext.Current.Session[".:LoggedInProfile:."] as Profile;
                    return profile.FullName;
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
                    //TODO: redirect to login
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
                    HttpContext.Current.Session[".:LoggedInProfile:."] = profile;
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