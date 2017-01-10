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
    public class ProfileDAO : BaseDAO<Profile, Guid>
    {
        public override void Save(Profile profile)
        {
            if (UserNameExists(profile.Username))
            {
                throw new ApplicationException("UserName already exists.");
            }
            profile.CreationDate = DateTime.Now;
            profile.LastLoginDate = (DateTime)System.Data.SqlTypes.SqlDateTime.Null;
            profile.Status = ProfileStatus.Enabled;

            base.Save(profile); 
        }

        public static bool UserNameExists(string username)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<Profile>()
            .Add(Restrictions.Eq("Username", username))
            .SetProjection(Projections.RowCount());
            
            var ProfileCount = Convert.ToInt32(criteria.UniqueResult());

            return (ProfileCount != 0); //return true if user exist ProfileCount !=0
        }
    }
}
