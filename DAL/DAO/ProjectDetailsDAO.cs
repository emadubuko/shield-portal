using CommonUtil.DBSessionManager;
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
    public class ProjectDetailsDAO : BaseDAO<ProjectDetails, int>
    {
        public ProjectDetails SearchByName(string projectTitle)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<ProjectDetails>()
            .Add(Restrictions.Eq("ProjectTitle", projectTitle));

            var result = criteria.UniqueResult<ProjectDetails>();

            return result;
        }


    }
}
