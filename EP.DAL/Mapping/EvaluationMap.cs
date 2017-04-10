using CommonUtil.Utilities;
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
            Map(m => m.ExpectedOutcome).Length(4001);
            HasMany(m => m.Activities)
                 .Cascade.SaveUpdate()
                .Inverse()
                .KeyColumns.Add("EvaluationId", mapping => mapping.Name("EvaluationId"));
            Map(m => m.DateCreated);
            Map(m => m.LastUpdatedDate);
            References(m => m.CreatedBy).Column("CreatedBy");
            Map(m => m.Status);
            Map(m => m.SupplementaryInfo).CustomType(typeof(XmlType<SupplementaryInfo>));
        }
    }
}
