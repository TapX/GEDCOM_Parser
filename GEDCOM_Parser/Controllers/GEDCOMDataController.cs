/* 
 * Controller for the database used to store uploaded GEDCOM files.
 * Note that this controller is never utilised in the normal operation of the web app.
 * It was created purely for development purposes.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */ 

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GEDCOM_Parser.Models;

namespace GEDCOM_Parser.Controllers
{
    public class GEDCOMDataController : Controller
    {
        private GedDBContext db = new GedDBContext();

        //
        // GET: /GEDCOMData/

        public ActionResult Index()
        {
            return View(db.GEDCOM_Data.ToList());
        }

        //
        // GET: /GEDCOMData/Details/5

        public ActionResult Details(string id = null)
        {
            GEDCOM_Data gedcom_data = db.GEDCOM_Data.Find(id);
            if (gedcom_data == null)
            {
                return HttpNotFound();
            }
            return View(gedcom_data);
        }

        //
        // GET: /GEDCOMData/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /GEDCOMData/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GEDCOM_Data gedcom_data)
        {
            if (ModelState.IsValid)
            {
                db.GEDCOM_Data.Add(gedcom_data);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(gedcom_data);
        }

        //
        // GET: /GEDCOMData/Edit/5

        public ActionResult Edit(string id = null)
        {
            GEDCOM_Data gedcom_data = db.GEDCOM_Data.Find(id);
            if (gedcom_data == null)
            {
                return HttpNotFound();
            }
            return View(gedcom_data);
        }

        //
        // POST: /GEDCOMData/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GEDCOM_Data gedcom_data)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gedcom_data).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(gedcom_data);
        }

        //
        // GET: /GEDCOMData/Delete/5

        public ActionResult Delete(string id = null)
        {
            GEDCOM_Data gedcom_data = db.GEDCOM_Data.Find(id);
            if (gedcom_data == null)
            {
                return HttpNotFound();
            }
            return View(gedcom_data);
        }

        //
        // POST: /GEDCOMData/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            GEDCOM_Data gedcom_data = db.GEDCOM_Data.Find(id);
            db.GEDCOM_Data.Remove(gedcom_data);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}