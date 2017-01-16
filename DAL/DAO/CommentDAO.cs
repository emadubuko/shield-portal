using DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DAO
{
    public class CommentDAO : BaseDAO<Comment,int>
    {
        public IList<Comment> SearchByDocumentId (Guid DocumentId)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<Comment>("c")
            .Add(Restrictions.Eq("DMPDocument.Id", DocumentId));

            criteria.SetProjection(
                Projections.Alias(Projections.Property("c.Message"), "Message"),
                Projections.Alias(Projections.Property("c.Id"), "Id"),
                Projections.Alias(Projections.Property("c.Commenter"), "Commenter"),
                Projections.Alias(Projections.Property("c.TagName"), "TagName"),
                Projections.Alias(Projections.Property("c.DateAdded"), "DateAdded")
                );

            var result = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(Comment))).List<Comment>();

           // var result = criteria.List<Comment>();
            return result;
        }
    }
}
