using CommonUtil.DBSessionManager;
using DAL.Entities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using System;
using System.Data.SqlClient;

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

            object orgid = DBNull.Value;
            if (obj.Organization != null)
            {
                orgid = obj.Organization.Id;
            }

            object leadMgr = DBNull.Value;
            if (obj.LeadActivityManager != null)
            {
                leadMgr = obj.LeadActivityManager.Id;
            }
             
            string updatescript = string.Format("Update [dmp_projectdetails] set ProjectTitle=@ProjectTitle, GrantReferenceNumber=@GrantReferenceNumber, ProjectStartDate=@ProjectStartDate, ProjectEndDate=@ProjectEndDate, LeadActivityManagerId=@LeadActivityManagerId, OrganizationId=@OrganizationId where id=@id ");
           
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand command = new SqlCommand(updatescript, s);

                command.Parameters.AddWithValue("ProjectTitle", GetDBValue(obj.ProjectTitle));
                command.Parameters.AddWithValue("GrantReferenceNumber", GetDBValue(obj.GrantReferenceNumber));
                command.Parameters.AddWithValue("ProjectStartDate", GetDBValue(obj.ProjectStartDate));
                command.Parameters.AddWithValue("ProjectEndDate", (obj.ProjectEndDate));
                command.Parameters.AddWithValue("LeadActivityManagerId", leadMgr);
                command.Parameters.AddWithValue("OrganizationId", orgid);
                command.Parameters.AddWithValue("id", obj.Id);

                int i = command.ExecuteNonQuery();                
            }
            return obj;            
        }
    }
}
