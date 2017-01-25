using BWReport.DAL.Entities;
using FluentNHibernate.Mapping;

namespace BWReport.DAL.Mapping
{
    public class ReportUploadMap : ClassMap<ReportUploads>
    {
        public ReportUploadMap()
        {
            Id(x => x.Id);
            Map(x => x.ReportName).Length(int.MaxValue);
            Map(x => x.DateUploaded);            
            Map(x => x.ReportingPeriodFrom);
            Map(x => x.ReportingPeriodTo);
            Map(x => x.UploadingUser);
            Map(x => x.FileLocation).Length(int.MaxValue);

        }
    }
}
