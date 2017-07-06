using CommonUtil.DBSessionManager;
using RADET.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;

namespace RADET.DAL.DAO
{
    public class RadetUploadErrorLogDAO  : BaseDAO<UploadLogError, int>
    {
        private void BulkSaveLog(List<UploadLogError> data)
        {
            string tableName = "radet_upload_log_error";
            var dt = new DataTable(tableName);

            dt.Columns.Add(new DataColumn("Status", typeof(string)));
            dt.Columns.Add(new DataColumn("RadetUploadId", typeof(int)));
            dt.Columns.Add(new DataColumn("ErrorDetails", typeof(string)));
           
            try
            {
                foreach (var tx in data)
                {
                    var row = dt.NewRow();

                    row["Status"] = GetDBValue(tx.Status);
                    row["RadetUploadId"] = GetDBValue(tx.RadetUpload.Id);
                    row["ErrorDetails"] = CommonUtil.Utilities.XMLUtil.ConvertToXml(tx.ErrorDetails);
                     
                    dt.Rows.Add(row);
                }
                DirectDBPostNew(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
