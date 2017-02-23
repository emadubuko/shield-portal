using System;
using BWReport.DAL.Entities;
using CommonUtil.DBSessionManager;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;

namespace BWReport.DAL.DAO
{
    public class ReportUploadsDao : BaseDAO<ReportUploads, long>
    {
        internal IList<ReportUploads> SearchPreviousUpload(string reportingPeriod,int year, string iP, string ReportName)
        {
            IList<ReportUploads> result = null;
            ISession session = BuildSession();

            ICriteria criteria = session.CreateCriteria<ReportUploads>()
                  .Add(Restrictions.Eq("ReportingPeriod", reportingPeriod))
                  .Add(Restrictions.Eq("FY", year))
                  .Add(Restrictions.Like("ImplementingPartner", iP, MatchMode.Anywhere))
                  .Add(Restrictions.Eq("ReportName", ReportName));

            result = criteria.List<ReportUploads>();

            return result;
        }
    }
}
