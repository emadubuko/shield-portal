using CommonUtil.Entities;
using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using System;

namespace CommonUtil.DAO
{
    public class LGADao : BaseDAO<LGA, long>
    {
        public LGA RetrievebyId(string name)
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
    }
}
