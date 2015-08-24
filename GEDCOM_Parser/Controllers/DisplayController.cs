/* 
 * Controller for the DISPLAY page. Displays links to all the contents in the GEDCOM file that has just been
 * uploaded. From this page, users can select which ever family/individual they wish to view in more detail.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GEDCOM_Parser.Models;

namespace GEDCOM_Parser.Controllers
{
    public class DisplayController : Controller
    {
        //
        // GET: /Display/
        private GedDBContext db = new GedDBContext();

        public ActionResult Index()
        {
            // Set up all lists and search queries required to display all of the summarised information
            var search_result = from entries in db.GEDCOM_Data select entries;
            var temp_result = from entries in db.GEDCOM_Data select entries;
            List<string> temp_list_s = new List<string>();
            List<string> temp_list_i = new List<string>();
            List<string> temp_list_i2 = new List<string>();
            List<string> temp_list_f = new List<string>();

            // Display link for GEDCOM header file information, if there is one to display (there can only be one set of header information per upload)
            ViewBag.head = false;
            if (search_result.Where(s => s.Tag.Contains("HEAD")).ToList().Count != 0)
            {
                ViewBag.head = true;
            }

            // Display links for GEDCOM file submitter information, if there are any to display
            ViewBag.subm = null;
            temp_result = search_result.Where(s => s.Level.Equals(0) && s.Tag.Contains("SUBM"));
            if (temp_result.ToList().Count != 0)
            {
                foreach (var row in temp_result.ToList())
                {
                    temp_list_s.Add(row.GEDCOM_ID); // Only the IDs of the submitters will be included in the display link
                }
            }
            ViewBag.subm = temp_list_s;

            // Display links for each individual included in the GEDCOM file, if there are any to display
            ViewBag.indi = null;
            temp_result = search_result.Where(s => s.Level.Equals(0) && s.Tag.Contains("INDI"));
            if (temp_result.ToList().Count != 0)
            {
                foreach (var row in temp_result.ToList())
                {
                    temp_list_i.Add(GetName(row.GEDCOM_ID) + " (" + row.GEDCOM_ID + ")"); // Display the name of the indivdual with the ID in brackets
                    temp_list_i2.Add(row.GEDCOM_ID);
                }
            }
            ViewBag.indi = temp_list_i;
            ViewBag.indi2 = temp_list_i2;

            // Display links for each family included in the GEDCOM file, if there are any to display
            ViewBag.fam = null;
            temp_result = search_result.Where(s => s.Level.Equals(0) && s.Tag.Contains("FAM"));
            if (temp_result.ToList().Count != 0)
            {
                foreach (var row in temp_result.ToList())
                {
                    temp_list_f.Add(row.GEDCOM_ID); // Only the IDs of the families will be included in the display link
                }
            }
            ViewBag.fam = temp_list_f;

            return View("Display");
        }

        // A function that returns that name of the indivual with a particular ID
        private string GetName(string ID)
        {
            var search_result = from entries in db.GEDCOM_Data select entries;
            List<GEDCOM_Data> temp_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID)).ToList();
            foreach (var temp_row in temp_results)
            {
                if (temp_row.Tag.Equals("NAME")) // Quite straight forward
                {
                    return temp_row.Data;
                }
            }
            return null;            
        }

    }
}
