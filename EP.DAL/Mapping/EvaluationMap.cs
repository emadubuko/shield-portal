using EP.DAL.Entities;
using FluentNHibernate.Mapping;

namespace EP.DAL.Mapping
{
    public class EvaluationMap : ClassMap<Evaluation>
    {

        public EvaluationMap()
        {
            Table("ep_Evaluation");

            Id(m => m.Id);
            References(m => m.ImplementingPartner).Column("ImplementingPartnerId");
            Map(m => m.ProgramName);
            Map(m => m.StartDate);
            Map(m => m.EndDate);
            Map(m => m.ExpectedOutcome);
            HasMany(m => m.Activities)
                 .Cascade.None()
                .Inverse()
                .KeyColumns.Add("ActivityId", mapping => mapping.Name("ActivityId"));
        }
    }
}
