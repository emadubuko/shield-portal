using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace CommonUtil.DBSessionManager
{

    public class NhibernateSessionManager
    {
        private const string SESSION_KEY = "::_SESSION_KEY_::";
        public ISessionFactory sessionFactory;

        public NhibernateSessionManager()
        {
            try
            {
                InitSessionFactory();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        public void RollbackSession()
        {
            ISession contextSession = ContextSession;
            if (contextSession != null)
            {
                if (contextSession.Transaction != null && contextSession.Transaction.IsActive)
                {
                    contextSession.Transaction.Rollback();
                }
            }
        }

        public void CloseSession()
        {
            ISession currentSession = CurrentSessionContext.Unbind(sessionFactory);

            currentSession.Close();
            currentSession.Dispose();

            ISession contextSession = ContextSession;
            if ((contextSession != null) && contextSession.IsOpen)
            {
                try
                {
                    contextSession.Flush();
                    if (contextSession.Transaction != null && contextSession.Transaction.IsActive)
                    {
                        contextSession.Transaction.Commit();
                    }
                }
                catch
                {
                    if (contextSession.Transaction != null && contextSession.Transaction.IsActive)
                    {
                        contextSession.Transaction.Rollback();
                    }
                }
                finally
                {
                    contextSession.Close();
                }
            }
            ContextSession = null;
        }


        public ISession GetSession()
        {
            if(sessionFactory == null)
            {
                InitSessionFactory();
            }
            ISession contextSession = null;
            if (CurrentSessionContext.HasBind(sessionFactory))
            {
                contextSession = sessionFactory.GetCurrentSession();
            }
            else //this is just for completeness sake
            {
                contextSession = ContextSession;
            }
            if (contextSession == null || contextSession.IsOpen == false)
            {
                contextSession = sessionFactory.OpenSession();
                CurrentSessionContext.Bind(contextSession);
                if (contextSession.Transaction == null || !contextSession.Transaction.IsActive)
                {
                    contextSession.BeginTransaction();
                }
                ContextSession = contextSession;
                
            }

            return contextSession;
        }

        private void InitSessionFactory()
        {
            Configuration cfg = new Configuration().Configure();
            FluentConfiguration fluentCfg = Fluently.Configure(cfg);

            NHibernate.Cfg.ConfigurationSchema.HibernateConfiguration hc
                = System.Configuration.ConfigurationManager.GetSection(NHibernate.Cfg.ConfigurationSchema.CfgXmlHelper.CfgSectionName) as NHibernate.Cfg.ConfigurationSchema.HibernateConfiguration;
            if (hc == null) throw new HibernateConfigException("Cannot process Hibernate Section in config file");
            if (hc.SessionFactory != null)
            {
                foreach (NHibernate.Cfg.ConfigurationSchema.MappingConfiguration mappingAssemblyCfg in hc.SessionFactory.Mappings)
                {
                    fluentCfg.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(mappingAssemblyCfg.Assembly)));
                }
            }

            //fluentCfg.CurrentSessionContext<CallSessionContext>();

            Configuration conf = fluentCfg.BuildConfiguration();
            new SchemaUpdate(conf).Execute(false, true);
            sessionFactory = conf.BuildSessionFactory();
        }

        //private void InitSessionFactory()
        //{
        //    string connectionString = "";
        //    if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseOnlineStorage"]))
        //    {
        //        connectionString = ConfigurationManager.ConnectionStrings["DMPOnlineDataStore"].ConnectionString;
        //    }
        //    else
        //    {
        //        connectionString = ConfigurationManager.ConnectionStrings["DMPLocalDataStore"].ConnectionString;
        //    }

        //    sessionFactory = Fluently.Configure()
        //   .Database(MsSqlConfiguration.MsSql2008.ConnectionString(connectionString))
        //   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DMPDocument>())
        //   .ExposeConfiguration(cfg => new SchemaUpdate(cfg)
        //       .Execute(false, true))
        //       .BuildSessionFactory();
        //}

        private static bool IsInWebContext()
        {
            return (HttpContext.Current != null);
        }

        private static ISession ContextSession
        {
            get
            {
                if (IsInWebContext())
                {
                    return (ISession)HttpContext.Current.Items["::_SESSION_KEY_::"];
                }
                return (ISession)CallContext.GetData("::_SESSION_KEY_::");
            }
            set
            {
                if (IsInWebContext())
                {
                    HttpContext.Current.Items["::_SESSION_KEY_::"] = value;
                }
                else
                {
                    CallContext.SetData("::_SESSION_KEY_::", value);
                }
            }
        }

        public static NhibernateSessionManager Instance
        {
            get
            {
                return Nested.SessionManager;
            }
        }

        private class Nested
        {
            internal static readonly NhibernateSessionManager SessionManager = new NhibernateSessionManager();
        }


    }

    //public class NhSessionManagementAttribute : ActionFilterAttribute
    //{
    //    public NhSessionManagementAttribute()
    //    {
    //        SessionFactory = WebApiApplication.SessionFactory;
    //    }

    //    private ISessionFactory SessionFactory { get; set; }

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
}
