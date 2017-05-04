using CommonUtil.DAO;
using CommonUtil.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    public class DQAFY2016Controller : Controller
    {
        // GET: DQAFY2016
        public ActionResult Index()
        {
            var profile = new Services.Utils().GetloggedInProfile();
            var dao = new AfenetReportDAO();
            IList<AfenetReport> report = null;
            if (User.IsInRole("shield_team") || (User.IsInRole("sys_admin")))
            {
                report = dao.RetrieveAll();
            }
            else
            {
                report = dao.RetrieveAll().Where(x => x.IP == profile.Organization.ShortName).ToList();
            }
            return View(report);
        }
    }
}