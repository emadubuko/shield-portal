using BWReport.DAL.Entities;
using FluentNHibernate.Mapping;

namespace BWReport.DAL.Mapping
{
    public class ReportUploadMap : ClassMap<ReportUploads>
    {
        public ReportUploadMap()
        {
            Table("bwr_ReportUploads");

            Id(x => x.Id);           
            Map(x => x.DateUploaded);
            Map(x => x.ReportingPeriod);
            Map(x => x.FY); 
            Map(x => x.UploadingUser); 
            Map(x => x.ImplementingPartner);
            Map(x => x.ReportName);
            
        }
    }
}
