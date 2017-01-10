using DAL.DAO;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
