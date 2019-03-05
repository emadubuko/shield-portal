using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.Linq;
using NHibernate.Persister.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

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

        public IQueryable<T> RetrieveAllLazily()
        {
            IQueryable<T> results = null;
            ISession session = BuildSession();
            try
            {
                results = session.Query<T>();
            }
            catch
            {
                throw;
            }
            return results;
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

        //public TimeSpan NHBulkInsert(List<T> ObjectListing)
        //{
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    ISessionFactory sessionFactory = BuildSession().SessionFactory;

        //    using (IStatelessSession session = sessionFactory.OpenStatelessSession())
        //    using (ITransaction transaction = session.BeginTransaction())
        //    {
        //        foreach (var obj in ObjectListing)
        //            session.Insert(obj);
        //        transaction.Commit();
        //    }

        //    stopwatch.Stop();
        //    var time = stopwatch.Elapsed;
        //    return time;
        //}
        


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

        public void DirectDBPostNew(DataTable dt, string tableName)
        {
            ISessionFactory sessionFactory = BuildSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                var s = (SqlConnection)connection;
                var copy = new SqlBulkCopy(s, SqlBulkCopyOptions.UseInternalTransaction, null);
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
            try
            {
                string val = Convert.ToString(value);
                if (val.Length > 50)
                {
                    value = val.Trim();
                }
            }
            catch (Exception) { }

            if (value == null)
            {
                return DBNull.Value;
            }
            return value;
        }

        /// <summary>
        /// Run and commit
        /// </summary>
        /// <param name="commandText"></param>
        public int RunSQL(string commandText)
        {
            int i = 0;
            if (!string.IsNullOrEmpty(commandText))
            {                
                IDbConnection cn = ((ISessionFactoryImplementor)BuildSession().SessionFactory).ConnectionProvider.GetConnection();
                IDbCommand cmd = new SqlCommand(commandText);
                cmd.Connection = (SqlConnection)cn;
                if (cn.State != ConnectionState.Open)
                    cn.Open();
                try
                {
                    cmd.CommandTimeout = 60 * 60;
                    //trans = cn.BeginTransaction();
                    //cmd.Transaction = trans;
                    i = cmd.ExecuteNonQuery();
                    //trans.Commit(); 
                }
                catch (Exception)
                {
                    //if (trans != null) trans.Rollback();
                    throw;
                }
                finally
                {
                    cn.Close(); 
                    cn.Dispose();
                    cmd.Dispose();
                }
            }
            return i;
        }

        private IDbTransaction _trans;

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="commandText"></param>
        public int StartSQL(string commandText)
        {
            if (!string.IsNullOrEmpty(commandText))
            {                 
                var conn = BuildSession().Connection; 
                IDbCommand cmd = new SqlCommand(); 
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                } 
                try
                {
                    _trans = conn.BeginTransaction();
                    cmd.Connection = conn;
                    cmd.Transaction = _trans;
                    cmd.CommandText = commandText; 
                    int i = cmd.ExecuteNonQuery();
                    return i;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            return 0;
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="commandText"></param>
        public void ContinueSQL(string commandText)
        {
            if (!string.IsNullOrEmpty(commandText))
            {
                IDbCommand cmd = new SqlCommand();
                try
                {
                    cmd.Connection = _trans.Connection;
                    cmd.Transaction = (SqlTransaction)_trans;
                    cmd.CommandText = commandText;

                    int i = cmd.ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    cmd.Dispose();
                }
            }
        }


        /// <summary>
        /// ExecuteNonQuery Commit
        /// </summary>
        public void CommitSQL()
        {
            if (_trans != null)
            {
                _trans.Commit();
            }
            if (_trans.Connection != null)
            {
                _trans.Connection.Close();
                _trans.Connection.Dispose();
            }
            _trans = null;
        }

        public void RollbackSQL()
        {
            if (_trans == null)
            {
                return;
            }
            if (_trans != null)
            {
                _trans.Rollback();
            }
            if (_trans.Connection != null)
            {
                _trans.Connection.Close();
                _trans.Connection.Dispose();
            }
            _trans = null;
        }


        public DataTable GetDatable(SqlCommand command)
        {
            string currentCmd = command.CommandText;
            var conn = (SqlConnection)((ISessionFactoryImplementor)BuildSession().SessionFactory).ConnectionProvider.GetConnection();
            command.Connection = conn;
            var dataTable = new DataTable();
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlDataAdapter da = new SqlDataAdapter(command); 
                da.Fill(dataTable);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                command.Dispose();
            }
            return dataTable;
        }

        public int executeDeleteMPM(SqlCommand command)
        {
            int i = 0;
            var conn = (SqlConnection)((ISessionFactoryImplementor)BuildSession().SessionFactory).ConnectionProvider.GetConnection();
            command.Connection = conn;
            var dataTable = new DataTable();
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                //SqlDataAdapter da = new SqlDataAdapter(command);
                //da.Fill(dataTable);
                 i = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                command.Dispose();
            }
           return i;
        }

    }
}
