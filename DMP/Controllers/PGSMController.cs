using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShieldPortal.Controllers
{
    [Authorize]

    public class PGSMController : Controller
    {
        // GET: PGSM
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult PreVisit()
        {
            return View();
        }

        public ActionResult QReport()
        {
            return View();
        }

        // GET: PGSM/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PGSM/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PGSM/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: PGSM/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PGSM/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: PGSM/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PGSM/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
