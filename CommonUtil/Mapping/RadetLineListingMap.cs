using CommonUtil.Entities;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonUtil.Mapping
{
    /*
   public class RadetLineListingMap : ClassMap<RadetTable>
    {
        public RadetLineListingMap()
        {
            Table("dqa_radet");

            Id(i => i.Id);
            Map(m => m.PatientId);
            Map(m => m.HospitalNo);
            Map(m => m.Sex);
            Map(m => m.Age_at_start_of_ART_in_years);
            Map(m => m.Age_at_start_of_ART_in_months);
            Map(m => m.ARTStartDate);
            Map(m => m.LastPickupDate);
            Map(m => m.MonthsOfARVRefill);
            Map(m => m.RegimenLineAtARTStart);
            Map(m => m.RegimenAtStartOfART);
            Map(m => m.CurrentRegimenLine); 
            Map(m => m.CurrentARTRegimen);
            Map(m => m.Pregnancy_Status);
            Map(m => m.Current_Viral_Load);
            Map(m => m.Date_of_Current_Viral_Load); 
            Map(m => m.Viral_Load_Indication);
            Map(m => m.CurrentARTStatus);

            Map(m => m.SelectedForDQA);
            Map(m => m.RadetYear); 
            References(m => m.IP).Column("IP");
            References(m => m.UploadReport).Column("UploadReportId");
        }
    }
    */
}
