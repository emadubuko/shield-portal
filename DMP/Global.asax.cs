using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace ShieldPortal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //PostAuthenticateRequest += Application_PostAuthenticateRequest;
        }

        protected void Application_Error()
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
                CommonUtil.Utilities.Logger.LogError(exception);
        }

        public override void Init()
        {
            PostAuthenticateRequest += MvcApplication_PostAuthenticateRequest;
            base.Init();
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            if (HttpContext.Current.Session != null)
            {
                AddUserToSession();
            }
        }
        

        private static readonly Dictionary<string, string> _sessions = new Dictionary<string, string>();
        private static readonly object padlock = new object();
        public void Session_Start(object sender, EventArgs e)
        {
            AddUserToSession();
        }

        public void Session_End(object sender, EventArgs e)
        {
            lock (padlock)
            {
                _sessions.Remove(Session.SessionID);
            }
        }
        

        void AddUserToSession()
        {
            lock (padlock)
            {
                if (!_sessions.ContainsValue(User.Identity.Name)) 
                {
                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        _sessions.Add(Session.SessionID, User.Identity.Name);//Session.SessionID);
                    }
                }
            }
        }

        public static Dictionary<string, string> Sessions
        {
            get
            {
                return _sessions;
            }
        }
    }
}
