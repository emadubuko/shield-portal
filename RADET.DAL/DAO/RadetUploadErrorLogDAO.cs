﻿using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Linq;
using RADET.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RADET.DAL.DAO
{
    public class RadetUploadErrorLogDAO  : BaseDAO<UploadLogError, int>
    {
        public IQueryable<UploadLogError> Search()
        {
            ISession session = BuildSession();
            var result = session.Query<UploadLogError>();//.Where(x => x.MetaData.Id == metadataId);
            return result;
        }
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
