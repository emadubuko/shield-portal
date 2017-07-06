using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

namespace CommonUtil.Utilities
{
    public class Utilities
    {
        public static string PasCaseConversion(string PascalWord)
        {
            return Regex.Replace(PascalWord, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

       public static bool IsAnyNullOrEmpty(object myObject)
        {
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(myObject, null);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
         

        public DataTable GenerateDataTable<T>(IEnumerable<T> entities, SingleTableEntityPersister classMapping) where T : IEntity
        {
            var _session = NhibernateSessionManager.Instance.GetSession();
            var generator = classMapping.IdentifierGenerator;

            var entityTable = new DataTable();
            var propertyNames = classMapping.PropertyNames;

            var identifierColumnNames = classMapping.IdentifierColumnNames.FirstOrDefault();
            if (identifierColumnNames != null)
            {
                entityTable.Columns.Add(identifierColumnNames, typeof(long));
            }

            var peristedProperties = propertyNames.Select((propertyName) =>
            {
                var propertyType = classMapping.GetPropertyType(propertyName);
                if (propertyType.IsCollectionType)
                {
                    return null;
                }
                var type = propertyType.ReturnedClass;
                if (propertyType.IsEntityType)
                {
                    type = typeof(long);
                }
                var columnName = classMapping.GetPropertyColumnNames(propertyName).FirstOrDefault();
                if (columnName == null)
                {
                    return null;
                }
                entityTable.Columns.Add(columnName, type);

                return new
                {
                    ColumnName = columnName,
                    PropertyName = propertyName,
                    IsEnum = propertyType.ReturnedClass.IsEnum,
                    Type = propertyType.ReturnedClass
                };
            }).Where(x => x != null);

            foreach (var entity in entities)
            {
                var row = entityTable.NewRow();

                if (identifierColumnNames != null)
                {
                    var value = (int)generator.Generate(_session.GetSessionImplementation(), null);
                    row[identifierColumnNames] = value;
                    entity.Id = value;
                }

                foreach (var persistedProperty in peristedProperties)
                {
                    var columnName = persistedProperty.ColumnName;
                    if (columnName != null)
                    {
                        object value = classMapping.GetPropertyValue(entity, persistedProperty.PropertyName, EntityMode.Poco);

                        if (value == null)
                        {
                            row[columnName] = DBNull.Value;
                        }
                        else
                        {
                            if (persistedProperty.IsEnum)
                            {
                                row[columnName] = Enum.GetName(persistedProperty.Type, value);
                            }
                            else if (value is IEntity)
                            {
                                row[columnName] = (value as IEntity).Id;
                            }
                            else
                            {
                                row[columnName] = value;
                            }
                        }
                    }
                }
                entityTable.Rows.Add(row);
            }
            return entityTable;
        }

        public virtual void BulkInsert<T>(IList<T> entities) where T : IEntity
        {
            var _session = NhibernateSessionManager.Instance.GetSession();
            var toUpdate = entities.Where(x => x.Id > 0);
            _session.SaveOrUpdate(toUpdate);

            var toInsert = entities.Where(x => x.Id == 0);
            _session.Flush();

            var allClassMetadata = _session.SessionFactory.GetAllClassMetadata();
            var classMapping = allClassMetadata.Values.Where(x => x is SingleTableEntityPersister)
                .Cast<SingleTableEntityPersister>()
                .FirstOrDefault(x => x.GetConcreteProxyClass(EntityMode.Poco) == typeof(T));

            using (var insertEntitiesCmd = new SqlCommand())
            {
                if (_session.Transaction != null && _session.Transaction.IsActive)
                {
                    _session.Transaction.Enlist(insertEntitiesCmd);
                }
                var entityTable = GenerateDataTable(toInsert, classMapping);
                var insertBulk = new SqlBulkCopy((SqlConnection)_session.Connection,
                    SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.FireTriggers, insertEntitiesCmd.Transaction)
                {
                    DestinationTableName = classMapping.TableName
                };
                foreach (DataColumn column in entityTable.Columns)
                {
                    insertBulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                insertBulk.WriteToServer(entityTable);
            }
        }

        public static string PasCaseConversion(object PascalWord)
        {
            if (PascalWord != null && !string.IsNullOrEmpty(Convert.ToString(PascalWord)))
                return PasCaseConversion(Convert.ToString(PascalWord));
            else
                return "";
        }

        public static string Sha256_Hash(string secret)
        {
            SHA256Managed crypt = new SHA256Managed();
            StringBuilder hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(secret), 0, Encoding.UTF8.GetByteCount(secret));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
        public static Dictionary<string, string> RetrieveDashboard(string role, string IPShortname, List<string> dashboardtype =null)
        {
            Dictionary<string, string> iframe = new Dictionary<string, string>();
            string script = "";
            if(dashboardtype != null)
            {
                script = string.Format("SELECT [DashboardFrame], [DashBoardType] FROM [PowerBiDashboard] where DashBoardType in '({0})' and rolename='{1}' and dashboardscope='{2}'", string.Join(",", dashboardtype), role, IPShortname);
            }
            else
            {
                script = string.Format("SELECT [DashboardFrame], [DashBoardType] FROM [PowerBiDashboard] where rolename='{0}' and dashboardscope='{1}'", role, IPShortname);
            }
            
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(script, s);
                SqlDataReader reader = null;
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    iframe.Add(reader[1] as string, reader[0] as string);
                }
                reader.Dispose();
            }
            return iframe;
        }


    }
}
