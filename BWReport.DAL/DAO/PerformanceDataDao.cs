using BWReport.DAL.Entities;
using CommonUtil.DBSessionManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWReport.DAL.DAO
{
    public class PerformanceDataDao : BaseDAO<PerformanceData, long>
    {
        public bool BulkInsert(List<PerformanceData> TargetMeasures)
        {
            string tableName = "PerformanceData";

            var dt = new DataTable(tableName);
            dt.Columns.Add(new DataColumn("StartPeriod", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("EndPeriod", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("HTC_TST", typeof(string)));
            dt.Columns.Add(new DataColumn("HTC_TST_POS", typeof(string)));
            dt.Columns.Add(new DataColumn("Tx_NEW", typeof(string)));
            dt.Columns.Add(new DataColumn("SDFId", typeof(long)));

            foreach (var tx in TargetMeasures)
            {
                var row = dt.NewRow();

                row["StartPeriod"] = GetDBValue(tx.ReportPeriodFrom);
                row["EndPeriod"] = GetDBValue(tx.ReportPeriodTo);
                row["HTC_TST"] = GetDBValue(tx.HTC_TST);
                row["HTC_TST_POS"] = GetDBValue(tx.HTC_TST_POS);
                row["Tx_NEW"] = GetDBValue(tx.Tx_NEW);
                row["SDFId"] = GetDBValue(tx.HealthFacility.Id);
                dt.Rows.Add(row);
            }
            DirectDBPost(dt, tableName);
            return true; //if we get this far, everything is ok
        }

    }
}
