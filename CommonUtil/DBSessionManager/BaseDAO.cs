using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CommonUtil.DBSessionManager
{
    public class BaseDAO<T, idT> where T : class
    {
        public virtual void Update(T obj)
        {
            ISession session = BuildSession();
            ITransaction tran = BuildTransaction(session);
            try
            {
                session.Clear();
                session.Update(obj);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }

        }


        public virtual void Save(T obj)
        {
            ISession session = BuildSession();
            ITransaction tran = BuildTransaction(session);
            try
            {
                session.Save(obj);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
        }


        public void Delete(T obj)
        {
            ISession session = BuildSession();
            ITransaction tran = BuildTransaction(session);
            try
            {
                session.Delete(obj);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
        }


        public List<T> RetrieveUsingPaging(ICriteria theCriteria, int startIndex, int maxRows, out int totalCount)
        {
            ISession session = BuildSession();
            ICriteria countCriteria = CriteriaTransformer.Clone(theCriteria).SetProjection(Projections.RowCount());
            ICriteria listCriteria = CriteriaTransformer.Clone(theCriteria).SetFirstResult(startIndex).SetMaxResults(maxRows);
            IList allResults = session.CreateMultiCriteria().Add<T>(listCriteria).Add(countCriteria).List();
            totalCount = Convert.ToInt32(((IList)allResults[1])[0]);
            return allResults[0] as List<T>;
        }


        public T Retrieve(idT id)
        {
            T result = default(T);
            ISession session = BuildSession();
            try
            {
                result = session.Get<T>(id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        public IList<T> RetrieveAll()
        {
            IList<T> results = null;
            ISession session = BuildSession();
            try
            {
                results = session.CreateCriteria<T>().List<T>();
            }
            catch
            {
                throw;
            }
            return results;
        }


        public static ISession BuildSession()
        {
            return NhibernateSessionManager.Instance.GetSession();
            //new NhibernateSessionManager().GetSession();
        }


        protected static ITransaction BuildTransaction(ISession session)
        {
            if (session.Transaction == null || !session.Transaction.IsActive)
            {
                return session.BeginTransaction();
            }
            return session.Transaction;
        }


        public void CommitChanges()
        {
            ISession session = BuildSession();
            if (session.Transaction != null && session.Transaction.IsActive)
            {
                session.Transaction.Commit();
            }
        }


        public void RollbackChanges()
        {
            ISession session = BuildSession();
            if (session.Transaction != null && session.Transaction.IsActive)
            {
                session.Transaction.Rollback();
            }
        }

        public void DirectDBPost(DataTable dt, string tableName)
        {
            ISessionFactory sessionFactory = BuildSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                var s = (SqlConnection)connection;
                var copy = new SqlBulkCopy(s);
                copy.BulkCopyTimeout = 10000;
                copy.DestinationTableName = tableName;
                foreach (DataColumn column in dt.Columns)
                {
                    copy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                copy.WriteToServer(dt);
            }
        }

        public static object GetDBValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            return value;
        }
         

    }
}
