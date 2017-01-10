using DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.DAO
{
    public class DMPDocumentDAO : BaseDAO<DMPDocument, Guid>
    {
        public DMPDocument GenericSearch(string SearchString)
        {
            var projection = Projections.SqlProjection("Cast(MyDMP as nvarchar(max)) AS str", new string[] { }, new NHibernate.Type.IType[] { });
            var criteria = Restrictions.Like(projection, SearchString, MatchMode.Anywhere);

            var session = BuildSession();
            var query = session.QueryOver<DMPDocument>().Where(criteria);
            var result = query.SingleOrDefault<DMPDocument>();
            return result;
        }

        public ICollection<DMPDocument> GenericSearchList(string SearchString)
        {
            var projection = Projections.SqlProjection("Cast(MyDMP as nvarchar(max)) AS str", new string[] { }, new NHibernate.Type.IType[] { });
            var criteria = Restrictions.Like(projection, SearchString, MatchMode.Anywhere);

            var session = BuildSession();
            var query = session.QueryOver<DMPDocument>().Where(criteria);
            var result = query.List<DMPDocument>();
            return result;
        }

        public ICollection<DMPDocument> SearchByAuthor(string Username)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<DMPDocument>();
            criteria.Add(Restrictions.Eq("InitiatorUsername", Username));
            var dmps = criteria.List<DMPDocument>();
            return dmps;
        }

        public ICollection<DMPDocument> SearchByDMP(int DmpId)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<DMPDocument>();
            criteria.Add(Restrictions.Eq("TheDMP.Id", DmpId));
            var dmps = criteria.List<DMPDocument>();
            return dmps;
        }
        public DMPDocument SearchMostRecentByDMP(int DmpId)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<DMPDocument>();
            criteria.Add(Restrictions.Eq("TheDMP.Id", DmpId));
            var dmps = criteria.List<DMPDocument>();
            return dmps.ToList().LastOrDefault();
        }
    }
}
