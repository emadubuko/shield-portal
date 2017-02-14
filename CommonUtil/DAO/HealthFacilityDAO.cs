using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;
using CommonUtil.Entities;

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
            dt.Columns.Add(new DataColumn("OrganizationType", typeof(string)));
            try
            {
                foreach (var tx in toSave)
                { 

                    var row = dt.NewRow();

                    row["Name"] = GetDBValue(tx.Name);
                    row["FacilityCode"] = GetDBValue(tx.FacilityCode);
                    row["LGAId"] = GetDBValue(tx.LGA.lga_code);
                    row["Longitude"] = GetDBValue(tx.Longitude);
                    row["Latitude"] = GetDBValue(tx.Latitude);
                    row["OrganizationType"] = GetDBValue(tx.OrganizationType);

                    dt.Rows.Add(row);
                }

                base.DirectDBPost(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
