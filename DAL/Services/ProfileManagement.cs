using CommonUtil.DAO;

namespace DAL.Services
{
    public class ProfileManagement
    {
       

        private void SendNotificationMail(string Name)
        {
            string mailBody = "Hi " + Name + @"<br/><br/>A profile for you has setup on the #ApplicationName#";
        }

        public bool UserNameExists(string Username)
        {
            return ProfileDAO.UserNameExists(Username);
        }
    }
}
