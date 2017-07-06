using CommonUtil.Utilities;
using FluentNHibernate.Mapping;
using RADET.DAL.Entities;
using System.Collections.Generic;

namespace RADET.DAL.Mapping
{
    public class UploadLogErrorMap : ClassMap<UploadLogError>
    {
        public UploadLogErrorMap()
        {

            Table("radet_upload_log_error");

            Id(x => x.Id);
            References(x => x.RadetUpload).Column("RadetUploadId");
            Map(x => x.ErrorDetails).CustomType(typeof(XmlType<List<ErrorDetails>>));
            Map(x => x.Status);
        }
    }
}
