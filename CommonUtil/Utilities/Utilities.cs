using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using ICSharpCode.SharpZipLib.Zip;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonUtil.Utilities
{
    public class Utilities
    {
        public static Dictionary<string, List<string>> GetQ4ARTSites(string IP)
        {
            string art_file = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/ART sites.xlsx");
            Dictionary<string, List<string>> artSites = new Dictionary<string, List<string>>();
            using (var package = new ExcelPackage(new FileInfo(art_file)))
            {
                var aSheet = package.Workbook.Worksheets["OriginalRADETFacilities"];

                for (int col = 2; col <= 776; col++)
                {
                    string lga = ExcelHelper.ReadCellText(aSheet, 1, col);
                    if (string.IsNullOrEmpty(lga))
                        break;

                    List<string> facilities = new List<string>();
                    int row = 2;
                    while (true)
                    {
                        var text = aSheet.Cells[row, col];
                        string facility = text.Text != null ? text.Text : ""; //ExcelHelper.ReadCellText(aSheet, row, 4);
                        if (string.IsNullOrEmpty(facility))
                            break;
                        facilities.Add(facility);
                        row++;
                    }
                    artSites.Add(lga, facilities);
                }
                //specific sheet for extrac facility not listed
                var specificSheet = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == IP + "_Extra");
                if (specificSheet != null)
                {
                    List<FACLGA> faclag = new List<FACLGA>();
                    int row = 2;
                    while (true)
                    {
                        var LGAtext = specificSheet.Cells[row, 2];
                        var factext = specificSheet.Cells[row, 3];
                        if (string.IsNullOrEmpty(factext.Text) || string.IsNullOrEmpty(LGAtext.Text))
                            break;

                        faclag.Add(new FACLGA
                        {
                            LGA = LGAtext.Text.Contains("Local Government Area") ? LGAtext.Text : LGAtext.Text.Trim() + " Local Government Area",
                            Facility = factext.Text,
                        });
                        row++;
                    }
                    foreach (var item in faclag.GroupBy(x => x.LGA))
                    {
                        if (artSites.ContainsKey(item.Key))
                        {
                            artSites[item.Key].AddRange(item.Select(x => x.Facility).ToList());
                        }
                        else
                        {
                            throw new ApplicationException("LGA is not valid");
                            //artSites.Add(item.Key, item.Select(x => x.Facility).ToList());
                        }
                    }
                }
            }
            return artSites;
        }

        public static Dictionary<string, string> GetARTSiteWithDATIMCode()
        {
            string art_file = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/ART sites.xlsx");
            //code and radet name
            Dictionary<string, string> artSites = new Dictionary<string, string>();
            using (var package = new ExcelPackage(new FileInfo(art_file)))
            {
                var aSheet = package.Workbook.Worksheets.FirstOrDefault();
                int row = 2;
                while (true)
                {
                    string r_name = ExcelHelper.ReadCellText(aSheet, row, 2);
                    if (string.IsNullOrEmpty(r_name))
                        break;
                    string d_code = ExcelHelper.ReadCellText(aSheet, row, 4);
                    if (string.IsNullOrEmpty(d_code.Trim()))
                        throw new ApplicationException("no code found");

                    if (d_code.Trim().Contains("#N/A") == false)
                        artSites.Add(d_code, r_name.Trim());
                    row++;
                }
            }
            return artSites;
        }

        public async Task ZipFolder(string filepath)
        {
            string[] filenames = Directory.GetFiles(filepath);

            using (ZipOutputStream s = new
            ZipOutputStream(File.Create(filepath + ".zip")))
            {
                s.SetLevel(9); // 0-9, 9 being the highest compression

                byte[] buffer = new byte[4096];

                foreach (string file in filenames)
                {

                    ZipEntry entry = new
                    ZipEntry(Path.GetFileName(file));

                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = await fs.ReadAsync(buffer, 0,
                            buffer.Length);

                            s.Write(buffer, 0, sourceBytes);

                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
        }


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


        public string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table class='table table-striped table-bordered table-hover' style='font-size:12px; width: 100%'>";
            //add header row
            html += "<thead style='background-color: #337ab7;color: #fff;'><tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                if (dt.Columns[i].DataType.Name.ToString().ToLower() == "decimal")
                {
                    html += "<td class=' sum'>" + dt.Columns[i].ColumnName + "</td>";
                }
                else if ("int32,int64".Contains(dt.Columns[i].DataType.Name.ToString().ToLower()))
                {
                    html += "<td class=' sum'>" + dt.Columns[i].ColumnName + "</td>";
                }
                else
                {
                    html += "<td>" + dt.Columns[i].ColumnName + "</td>";
                }
            html += "</tr></thead>";

            //add rows
            html += "<tbody>";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    if (dt.Columns[j].DataType.Name.ToString().ToLower() == "decimal")
                    {
                        html += "<td class=' sum'>" + string.Format("{0:N2}", dt.Rows[i][j]) + "</td>";
                    }
                    else if ("int32,int64".Contains(dt.Columns[j].DataType.Name.ToString().ToLower()))
                    {
                        html += "<td class=' sum'>" + string.Format("{0:N0}", dt.Rows[i][j]) + "</td>";
                    }
                    else
                    {
                        html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                    }
                html += "</tr>";
            }
            html += "</tbody></table>";
            return html;
        }

        private class FACLGA
        {
            public string Facility { get; set; }
            public string LGA { get; set; }
        }
    }

}
