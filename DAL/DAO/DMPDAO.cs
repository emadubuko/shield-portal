using CommonUtil.DBSessionManager;
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

        public IList<DMP> SearchOrganizaionId(int orgId)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<DMP>()
            .Add(Restrictions.Eq("Organization.Id", orgId))
            .Add(Restrictions.IsNotNull("TheProject.Id"));

            var result = criteria.List<DMP>();

            return result;
        }

        public IList<DMP> RetrieveDMPSorted()
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<DMP>() 
            .Add(Restrictions.IsNotNull("TheProject.Id"))
            .AddOrder(Order.Desc("DateCreated"));

            var result = criteria.List<DMP>();

            return result;
        }

    }
}
