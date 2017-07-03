using FluentNHibernate.Mapping;
using RADET.DAL.Entities;

namespace RADET.DAL.Mapping
{
    public  class RadetPatientMap :  ClassMap<RadetPatient>
    {
        public RadetPatientMap()
        {
            Table("radet_patient");

            Id(x => x.Id);
            Map(m => m.PatientId);
            Map(m => m.HospitalNo);
            Map(m => m.Sex);
            Map(m => m.Age_at_start_of_ART_in_years);
            Map(m => m.Age_at_start_of_ART_in_months);

            HasMany(x => x.PatientLineListing)
            .Cascade.SaveUpdate()
                .Inverse()
                .KeyColumns.Add("RadetPatientId", mapping => mapping.Name("RadetPatientId"));
        }
    }
}
