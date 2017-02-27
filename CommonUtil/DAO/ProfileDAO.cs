using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;

namespace CommonUtil.DAO
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

        public Profile GetProfileByUsername(string username)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<Profile>()
            .Add(Restrictions.Eq("Username", username)); 
            var profile = criteria.UniqueResult<Profile>();

            return profile;
        }
        public IList<Profile> GetProfilesByIP(int IPId)
        {
            var session = BuildSession();
            ICriteria criteria = session.CreateCriteria<Profile>()
            .Add(Restrictions.Eq("Organization.Id", IPId));
            var profile = criteria.List<Profile>();

            return profile;
        }
    }
}
