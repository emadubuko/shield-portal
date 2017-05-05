using CommonUtil.Entities;
using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using System;

namespace CommonUtil.DAO
{
    public class LGADao : BaseDAO<LGA, string>
    {
        public LGA Retrievebyname(string name)
        {
            LGA lga = null;
            ISession session = BuildSession();
            try
            {
                ICriteria criteria = session.CreateCriteria<LGA>().Add(Expression.Eq("Name", name));
                lga = criteria.UniqueResult<LGA>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lga;
        }

        public LGA RetrievebyLGA_State(string lga, string state)
        {
            ISession session = BuildSession();
            try
            {
                ICriteria criteria = session.CreateCriteria<LGA>().Add(Expression.Eq("lga_name", lga));
                criteria.CreateCriteria("State", "st", NHibernate.SqlCommand.JoinType.InnerJoin)
               .Add(Restrictions.Eq("state_code", state));
                return criteria.UniqueResult<LGA>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
