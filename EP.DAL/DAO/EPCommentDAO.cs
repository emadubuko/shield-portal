using CommonUtil.DBSessionManager;
using EP.DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace EP.DAL.DAO
{
    public class EPCommentDAO : BaseDAO<EPComment, int>
    {
        public IList<EPComment> SearchByDocumentId(string ResultTagId)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<EPComment>("c")
            .Add(Restrictions.Eq("ResultTagId", ResultTagId));

            criteria.SetProjection(
                Projections.Alias(Projections.Property("c.Message"), "Message"),
                Projections.Alias(Projections.Property("c.Id"), "Id"),
                Projections.Alias(Projections.Property("c.Commenter"), "Commenter"),
                Projections.Alias(Projections.Property("c.ResultTagId"), "ResultTagId"),
                Projections.Alias(Projections.Property("c.DateAdded"), "DateAdded")
                );

            var result = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(EPComment))).List<EPComment>();
            return result;
        }
    }
}
