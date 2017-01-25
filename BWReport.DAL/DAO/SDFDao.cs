using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using BWReport.DAL.Entities;

namespace BWReport.DAL.DAO
{
    public class SDFDao : BaseDAO<HealthFacility, long>
    {
        public HealthFacility RetrievebyName(string name)
        {
            HealthFacility sdf = null;
            ISession session = BuildSession();

            ICriteria criteria = session.CreateCriteria<HealthFacility>().Add(Expression.Eq("Name", name));
            sdf = criteria.UniqueResult<HealthFacility>();

            return sdf;
        }

        public void BulkInsert(List<HealthFacility> facilities)
        {
            foreach (var tx in facilities)
            {
                var sdf = RetrievebyName(tx.Name);
                if (sdf == null)
                    Save(tx);
                else
                    tx.Id = sdf.Id;
            }


            //string tableName = "SDF";

            //var dt = new DataTable(tableName);
            //dt.Columns.Add(new DataColumn("Name", typeof(string)));
            //dt.Columns.Add(new DataColumn("StateId", typeof(long)));
            //dt.Columns.Add(new DataColumn("LGAId", typeof(long)));
            //dt.Columns.Add(new DataColumn("Longitude", typeof(string)));
            //dt.Columns.Add(new DataColumn("Latitude", typeof(string)));
            //try
            //{
            //    foreach (var tx in facilities)
            //    {
            //        if(RetrievebyName(tx.Name) !=null)
            //        {                         
            //            continue;
            //        }

            //        var row = dt.NewRow();

            //        row["Name"] = GetDBValue(tx.Name);
            //        row["StateId"] = GetDBValue(tx.State.ID);
            //        row["LGAId"] = GetDBValue(tx.LGA.ID);
            //        row["Longitude"] = GetDBValue(tx.Longitude);
            //        row["Latitude"] = GetDBValue(tx.Latitude);
            //        dt.Rows.Add(row);
            //    }

            //    base.DirectDBPost(dt, tableName);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }
    }
}
