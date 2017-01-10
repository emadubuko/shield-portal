using DAL.Entities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace DAL.DAO
{

    public class NhibernateSessionManager
    {
        private const string SESSION_KEY = "::_SESSION_KEY_::";
        private ISessionFactory sessionFactory;

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
            ISession contextSession = this.ContextSession;
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
            ISession contextSession = this.ContextSession;
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
            this.ContextSession = null;
        }


        public ISession GetSession()
        {
            ISession contextSession = ContextSession;
            if (contextSession == null || contextSession.IsOpen == false)
            {
                contextSession = this.sessionFactory.OpenSession();
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
            string connectionString = "";
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["UseOnlineStorage"]))
            {
                connectionString = ConfigurationManager.ConnectionStrings["DMPOnlineDataStore"].ConnectionString;
            }
            else
            {
                connectionString = ConfigurationManager.ConnectionStrings["DMPLocalDataStore"].ConnectionString;
            }
            
            sessionFactory = Fluently.Configure()
           .Database(MsSqlConfiguration.MsSql2008.ConnectionString(connectionString))
           .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DMPDocument>())
           .ExposeConfiguration(cfg => new SchemaUpdate(cfg)
               .Execute(false, true))
               .BuildSessionFactory();
        }

        private bool IsInWebContext()
        {
            return (HttpContext.Current != null);
        }

        private ISession ContextSession
        {
            get
            {
                if (this.IsInWebContext())
                {
                    return (ISession)HttpContext.Current.Items["::_SESSION_KEY_::"];
                }
                return (ISession)CallContext.GetData("::_SESSION_KEY_::");
            }
            set
            {
                if (this.IsInWebContext())
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
}
