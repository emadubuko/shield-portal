using System;
using CommonUtil.DBSessionManager;
using EP.DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace EP.DAL.DAO
{
    public class EvaluationDAO : BaseDAO<Evaluation, int>
    {
        public IList<Evaluation> SearchByProgramName(string programName, int IP_id)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<Evaluation>("c")
            .Add(Restrictions.Eq("ImplementingPartner.Id", IP_id))
            .Add(Restrictions.Like("ProgramName", programName, MatchMode.Anywhere));

           var result = criteria.List<Evaluation>();
            return result;
        }
    }
}
