using FluentNHibernate.Mapping;
using RADET.DAL.Entities;

namespace RADET.DAL.Mapping
{
    public class RadetMetaDataMap : ClassMap<RadetMetaData>
    {
        public RadetMetaDataMap()
        {
            Table("radet_MetaData");

            Id(i => i.Id);

            References(x => x.IP).Column("IP");
            References(x => x.FY12_17_IP).Column("FY12_17_IP");
            Map(x => x.RadetPeriod);
            Map(x => x.Facility);
            Map(x => x.FacilityDatimCode);
            References(x => x.LGA).Column("LGA");
            Map(x => x.Supplementary);
            HasMany(x => x.PatientLineListing)
                .Cascade.None()
               .Inverse()
               .KeyColumns.Add("RadetMetaDataId", mapping => mapping.Name("RadetMetaDataId"));

            References(r => r.RadetUpload).Column("RadetUploadId");
        }
    }
}
