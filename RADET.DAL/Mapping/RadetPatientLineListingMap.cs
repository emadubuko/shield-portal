﻿using FluentNHibernate.Mapping;
using RADET.DAL.Entities;

namespace RADET.DAL.Mapping
{
    public class RadetPatientLineListingMap : ClassMap<RadetPatientLineListing>
    {
        public RadetPatientLineListingMap()
        {
            Table("radet_patient_line_listing");

            Id(i => i.Id);
            References(x => x.RadetPatient).Column("RadetPatientId");
            Map(m => m.ARTStartDate);
            Map(m => m.LastPickupDate);
            Map(m => m.MonthsOfARVRefill);
            Map(m => m.RegimenLineAtARTStart);
            Map(m => m.RegimenAtStartOfART);
            Map(m => m.CurrentRegimenLine);
            Map(m => m.CurrentARTRegimen);
            Map(m => m.PregnancyStatus);
            Map(m => m.CurrentViralLoad);
            Map(m => m.DateOfCurrentViralLoad);
            Map(m => m.ViralLoadIndication);
            Map(m => m.CurrentARTStatus);

            Map(m => m.SelectedForDQA);
            Map(m => m.RadetPeriod); 
            References(m => m.MetaData).Column("RadetMetaDataId");
        }
    }
}

 
