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
            ICriteria criteria = session.CreateCriteria<Comment>()
            .Add(Restrictions.Eq("DMPDocument.Id", DocumentId));

            var result = criteria.List<Comment>();
            return result;
        }
    }
}
