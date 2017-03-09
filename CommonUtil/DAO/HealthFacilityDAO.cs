using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;
using CommonUtil.Entities;
using CommonUtil.Utilities;

namespace CommonUtil.DAO
{
    public class HealthFacilityDAO : BaseDAO<HealthFacility, int>
    {
        public HealthFacility RetrievebyName(string name)
        {
            HealthFacility sdf = null;
            ISession session = BuildSession();

            ICriteria criteria = session.CreateCriteria<HealthFacility>().Add(Expression.Eq("Name", name));
            sdf = criteria.UniqueResult<HealthFacility>();

            return sdf;
        }

        public HealthFacility RetrievebyCode(string code)
        {
            HealthFacility sdf = null;
            ISession session = BuildSession();

            ICriteria criteria = session.CreateCriteria<HealthFacility>().Add(Expression.Eq("FacilityCode", code));
            sdf = criteria.UniqueResult<HealthFacility>();

            return sdf;
        }

        public void BulkInsert(List<HealthFacility> facilities)
        {
            var toSave = facilities.Where(x => x.Id == 0);

            string tableName = "HealthFacility";

            var dt = new DataTable(tableName);
            dt.Columns.Add(new DataColumn("Name", typeof(string)));
            dt.Columns.Add(new DataColumn("FacilityCode", typeof(string)));
            dt.Columns.Add(new DataColumn("LGAId", typeof(string)));
            dt.Columns.Add(new DataColumn("Longitude", typeof(string)));
            dt.Columns.Add(new DataColumn("Latitude", typeof(string)));
            dt.Columns.Add(new DataColumn("ImplementingPartnerId", typeof(int)));
            dt.Columns.Add(new DataColumn("OrganizationType", typeof(string)));

            try
            {
                foreach (var tx in toSave)
                {
                    try
                    {
                        var row = dt.NewRow();

                        row["Name"] = GetDBValue(tx.Name);
                        row["FacilityCode"] = GetDBValue(tx.FacilityCode);
                        row["LGAId"] = GetDBValue(tx.LGA.lga_code);
                        row["Longitude"] = GetDBValue(tx.Longitude);
                        row["Latitude"] = GetDBValue(tx.Latitude);
                        row["ImplementingPartnerId"] = GetDBValue(tx.Organization.Id);
                        row["OrganizationType"] = GetDBValue(tx.OrganizationType);

                        dt.Rows.Add(row);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                base.DirectDBPost(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveFromCSV(string[] theLines)
        {
            try
            {
                var hfDAO = new HealthFacilityDAO();
                var existingFacilities = hfDAO.RetrieveAll()
                    .ToDictionary(x => x.FacilityCode);
                var Ips = new OrganizationDAO().RetrieveAll();
                var lgas = new LGADao().RetrieveAll().GroupBy(x => x.State.state_code);

                List<HealthFacility> hfs = new List<Entities.HealthFacility>();

                foreach (var item in theLines)
                {
                    string[] entries = item.Split(',');
                    string healthFacilityCode = entries[2];

                    HealthFacility facility = null;
                    existingFacilities.TryGetValue(healthFacilityCode, out facility);
                    if (facility == null)
                    {
                        string statecode = entries[7].Trim();
                        string lganame = entries[6].ToLower().Trim();
                        var ip = Ips.FirstOrDefault(x => x.ShortName == entries[8].Trim());
                        var lga = lgas.FirstOrDefault(x => x.Key == statecode).ToList()
                            .FirstOrDefault(y => y.lga_name.ToLower().Trim() == lganame);
                        if(lga == null || ip == null)
                        {
                            throw new Exception(lga == null ? "Invalid LGA /state combination" : "Invalid IP shortName");
                        }
                        else
                        {
                            facility = new HealthFacility
                            {
                                FacilityCode = healthFacilityCode,
                                LinkCode = healthFacilityCode.Contains('-') ? healthFacilityCode.Split('-')[1] : "",
                                Name = entries[9],
                                Organization = ip,
                                LGA = lga,
                            };
                            hfs.Add(facility);
                        }                        
                    }
                }
                BulkInsert(hfs);
                return "";
            }
            catch(Exception ex)
            {
                Logger.LogError(ex);
                return  ex.Message;
            }           
        }
    }
}
