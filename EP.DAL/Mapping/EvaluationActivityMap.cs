using CommonUtil.Utilities;
using EP.DAL.Entities;
using FluentNHibernate.Mapping;

namespace EP.DAL.Mapping
{
    public class EvaluationActivityMap : ClassMap<EvaluationActivities>
    {
        public EvaluationActivityMap()
        {
            Table("ep_activities");

            Id(i => i.Id);
            Map(m => m.Name);
            Map(m => m.StartDate);
            Map(m => m.EndDate);
            Map(m => m.ExpectedOutcome);
            References(r => r.TheEvaluation).Column("EvaluationId");
            Map(m => m.Results).CustomType(typeof(XmlType<ActivityResult>));
            //.Cascade.None()                .KeyColumn("") 
        }
    }
}
