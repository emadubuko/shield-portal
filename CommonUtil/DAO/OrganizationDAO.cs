using System;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using NHibernate;
using NHibernate.Criterion;

namespace CommonUtil.DAO
{
    public class OrganizationDAO : BaseDAO<Organizations, int>
    {
        public Organizations SearchByShortName(string shortName)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<Organizations>()
            .Add(Restrictions.Like("ShortName", shortName, MatchMode.Anywhere));
            var Ip = criteria.UniqueResult<Organizations>();

            return Ip;
        }
    }
}
