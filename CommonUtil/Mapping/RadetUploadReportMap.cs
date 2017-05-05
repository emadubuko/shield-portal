using CommonUtil.Entities;
using FluentNHibernate.Mapping;

namespace CommonUtil.Mapping
{
    public class RadetUploadReportMap : ClassMap<RadetUploadReport>
    {
        public RadetUploadReportMap()
        {
            Table("dqa_radet_upload_report");

            Id(i => i.Id); 
            References(m => m.UploadedBy);
            References(m => m.IP).Column("IP");
            Map(m => m.dqa_year);
            Map(m => m.dqa_quarter);
            Map(m => m.DateUploaded);
            Map(x => x.CurrentYearTx_New).Column("Current_year_tx_new");
            Map(m => m.Facility);
            HasMany(h => h.Uploads).Cascade.SaveUpdate().Inverse()
                .KeyColumns.Add("UploadReportId", mapping => mapping.Name("UploadReportId"));
        }
    }
}
