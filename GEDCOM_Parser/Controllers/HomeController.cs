/* 
 * Controller for the HOME page. Simply returns the home page view.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GEDCOM_Parser.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
