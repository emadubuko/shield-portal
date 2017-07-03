using FluentNHibernate.Mapping;
using RADET.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADET.DAL.Mapping
{
    public class RadetMetaDataMap : ClassMap<RadetMetaData>
    {
        public RadetMetaDataMap()
        {
            Id(i => i.Id);

            References(x => x.IP).Column("IP");
            Map(x => x.RadetPeriod);
            Map(x => x.Facility);
            References(x => x.LGA).Column("LGA");

            HasMany(x => x.PatientLineListing)
                .Cascade.SaveUpdate()
               .Inverse()
               .KeyColumns.Add("RadetMetaDataId", mapping => mapping.Name("RadetMetaDataId"));

            References(r => r.RadetUpload).Column("RadetUploadId");
        }
    }
}
