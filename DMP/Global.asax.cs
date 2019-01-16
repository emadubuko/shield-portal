using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using CommonUtil.DBSessionManager;
using CommonUtil.Utilities;
using NHibernate.Context;

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
            //GlobalConfiguration.Configuration.Filters.Add(new NhSessionManagementAttribute());
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
            if (Request.Url.AbsoluteUri.ToLower().Contains("https://portal.shieldnigeriaproject.com"))
            {
                Response.Redirect("https://mer.shieldnigeriaproject.com/", true);
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


    //public class NhSessionManagementAttribute : System.Web.Http.Filters.ActionFilterAttribute
    //{
    //    public NhSessionManagementAttribute()
    //    {
    //        SessionFactory = CommonUtil.DBSessionManager.NhibernateSessionManager.sessionFactory;
    //    }

    //    private NHibernate.ISessionFactory SessionFactory { get; set; }

    //    public override void OnActionExecuting(HttpActionContext actionContext)
    //    {
    //        var session = SessionFactory.OpenSession();
    //        CurrentSessionContext.Bind(session);
    //        session.BeginTransaction();
    //    }

    //    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    //    {
    //        var session = SessionFactory.GetCurrentSession();
    //        var transaction = session.Transaction;
    //        if (transaction != null && transaction.IsActive)
    //        {
    //            transaction.Commit();
    //        }
    //        session = CurrentSessionContext.Unbind(SessionFactory);
    //        session.Close();
    //    }
    //}

    //public class SessionManagementAttribute : Attribute, IControllerConfiguration
    //{
    //    public void Initialize(HttpControllerSettings controllerSettings,
    //                           HttpControllerDescriptor controllerDescriptor)
    //    {
    //        controllerDescriptor.Configuration.Filters.Add(new NhSessionManagementAttribute());
    //    }

    //}


    public class MyHttpModule : IHttpModule
    {

        public MyHttpModule()
        {
            SessionFactory = NhibernateSessionManager.Instance.sessionFactory;
        }

        public void Init(HttpApplication application)
        {
            try
            {
                application.BeginRequest += new EventHandler(this.context_BeginRequest);
                application.EndRequest += new EventHandler(this.context_EndRequest);
            }
            catch
            {
                Logger.LogInfo("crashed from Init","");
            }
            
        }

        private NHibernate.ISessionFactory SessionFactory { get; set; }
        public void context_BeginRequest(object sender, EventArgs e)
        {
            var session = SessionFactory.OpenSession();
            CurrentSessionContext.Bind(session);
            session.BeginTransaction();
        }

        public void context_EndRequest(object sender, EventArgs e)
        {
            try
            {
                if (SessionFactory != null && CurrentSessionContext.HasBind(SessionFactory))
                {
                    var session = SessionFactory.GetCurrentSession();
                    var transaction = session.Transaction;
                    if (transaction != null && transaction.IsActive)
                    {
                        transaction.Commit();
                    }
                    session = CurrentSessionContext.Unbind(SessionFactory);
                    session.Close();
                }
            }
            catch(Exception ex)
            {
                Logger.LogInfo("crashed on end_request", "");
                Logger.LogError(ex);
            }            
        }

        public void Dispose() { }
    }
}
