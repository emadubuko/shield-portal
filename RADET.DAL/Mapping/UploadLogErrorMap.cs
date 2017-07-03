using CommonUtil.Utilities;
using FluentNHibernate.Mapping;
using RADET.DAL.Entities;

namespace RADET.DAL.Mapping
{
    public class UploadLogErrorMap : ClassMap<UploadLogError>
    {
        public UploadLogErrorMap()
        {

            Table("radet_upload_log_error");

            Id(x => x.Id);
            References(x => x.RadetUpload).Column("RadetUploadId");
            Map(x => x.ErrorDetails).CustomType(typeof(XmlType<ErrorDetails>));
            Map(x => x.Status);
        }
    }
}
