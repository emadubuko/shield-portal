using DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace DAL.DAO
{
    public class DMPDAO : BaseDAO<DMP, int>
    {
        public DMP SearchByName(string dmpTitle)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<DMP>()
            .Add(Restrictions.Eq("DMPTitle", dmpTitle));

            var result = criteria.UniqueResult<DMP>();

            return result;
        }
    }
}
