using FluentNHibernate.Mapping;
using RADET.DAL.Entities;

namespace RADET.DAL.Mapping
{
    public class RadetUploadMap : ClassMap<RadetUpload>
    {
        public RadetUploadMap()
        {
            Table("radet_upload");

            Id(i => i.Id);
            References(x => x.IP).Column("IP");
            References(x => x.UploadedBy).Column("UploadedBy"); 
            Map(x => x.DateUploaded); 
            HasMany(x => x.RadetMetaData)
               .Cascade.SaveUpdate()
              .Inverse()
              .KeyColumns.Add("RadetUploadId", mapping => mapping.Name("RadetUploadId"));
        }
    }
}
