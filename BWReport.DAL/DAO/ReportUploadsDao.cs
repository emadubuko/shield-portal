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
        /// <summary>
        /// the file name is to take care of ccfn that has different files 
        /// </summary>
        /// <param name="reportingPeriod"></param>
        /// <param name="year"></param>
        /// <param name="iP"></param>
        /// <param name="ReportName"></param>
        /// <returns></returns>
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
