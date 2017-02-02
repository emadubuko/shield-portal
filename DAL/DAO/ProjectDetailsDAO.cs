using CommonUtil.DBSessionManager;
using DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public virtual ProjectDetails ExplicitUpdate(ProjectDetails obj)
        {
            if (obj == null)
                return obj;

            string orgid = obj.Organization ==null ? "NULL": obj.Organization.Id.ToString();
            string leadMgr = obj.LeadActivityManager == null ? "NULL" : obj.LeadActivityManager.Id.ToString();

            string updatescript = string.Format("Update [dmp_projectdetails] set ProjectTitle='{0}', GrantReferenceNumber='{1}', ProjectStartDate='{2}', ProjectEndDate='{3}', LeadActivityManagerId='{4}', OrganizationId='{5}' where id='{6}' ",
                                                                       obj.ProjectTitle, obj.GrantReferenceNumber,obj.ProjectStartDate, obj.ProjectEndDate, leadMgr, orgid, obj.Id);
            
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(updatescript, s);                
                int i = selectCommand.ExecuteNonQuery();                
            }
            return obj;            
        }
    }
}
