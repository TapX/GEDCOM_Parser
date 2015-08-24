/* 
 * Controller for the QUERY page. The function of this page is to display elaborated information
 * on any GEDCOM link the user selects in the DISPLAY page (including header, submitter, individual
 * or family information). This is where the majority of the data parsing occurs - translating
 * seemingly useless GEDCOM jargon into useful, easy-to-understand information. Note that all GEDCOM
 * data parsed here has already been read and uploaded into the database.
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
    public class QueryController : Controller
    {
        //
        // GET: /Query/

        private GedDBContext db = new GedDBContext();

        // The GEDCOM ID and Tag must be sent in order to parse the correct DB entry
        public ActionResult Index(string ID, string Tag)
        {
            //Initialise all strings and lists used to parse GEDCOM DB entries

            var search_result = from entries in db.GEDCOM_Data select entries;
            List<string> title_list =  new List<string>();
            List<string> content_list = new List<string>();
            List<GEDCOM_Data> query_results = new List<GEDCOM_Data>();
            List<GEDCOM_Data> fam_query_results = new List<GEDCOM_Data>();
            string tag_expanded = "";
            string temp_content = "";

            // Parsing occurs differently for GEDCOM header, submitter, individual and family entries
            switch (Tag)
            {
                // For HEAD data: general GEDCOM file information
                case "HEAD":
                {
                    title_list.Add("Your GEDCOM file's info:");
                    query_results = search_result.Where(s => s.GEDCOM_ID.Equals("HEAD")).ToList(); // Find all of the relevant entries in the DB
                    foreach (var row in query_results)
                    {
                        if (row.Level != 0) // We don't want to display the 'HEAD' entry --- it adds no value in this case
                        {
                            tag_expanded = db.Tag_Conv.Find(row.Tag).Tag_Expanded; // Translate the tag from GEDCOM jargon to English
                            temp_content += ("".PadRight(5*row.Level) + tag_expanded + ":").PadRight(50) + row.Data + "\n"; // Indent the entry according to its level and add the data
                        }
                    }
                    content_list.Add(temp_content); // Add the parsed data to the content list that will be displayed on the page
                    break;
                }

                // For SUBM data: information on the submitter of the GEDCOM file
                case "SUBM":
                {
                    query_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID)).ToList(); // Find all of the relevant entries in the DB
                    foreach (var row in query_results)
                    {
                        if (row.Level != 0) // We don't want to display the first entry as data --- we'll make that the title heading later
                        {
                            tag_expanded = db.Tag_Conv.Find(row.Tag).Tag_Expanded; // Translate the tag from GEDCOM jargon to English
                            temp_content += ("".PadRight(5 * row.Level) + tag_expanded + ":").PadRight(50) + row.Data + "\n"; // Indent the entry according to its level and add the data
                        }
                        else // Make the first entry (the one with level 0) the title
                        {
                            title_list.Add("Info of submitter with ID: " + row.GEDCOM_ID);
                        }
                    }
                    content_list.Add(temp_content); // Add the parsed data to the content list that will be displayed on the page
                    break;
                }

                // For INDI data: individuals whose information is represented in the GEDCOM file
                case "INDI":
                {
                    query_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID)).ToList(); // Find all of the relevant entries in the DB
                    foreach (var row in query_results)
                    {
                        if (row.Level != 0) // We don't want to display the first entry as data --- we'll make that the title heading later
                        {
                            tag_expanded = db.Tag_Conv.Find(row.Tag).Tag_Expanded; // Translate the tag from GEDCOM jargon to English
                            temp_content += ("".PadRight(5 * row.Level) + tag_expanded + ":").PadRight(50) + row.Data + "\n"; // Indent the entry according to its level and add the data
                        }
                        else // Make the first entry (the one with level 0) the title
                        {
                            title_list.Add(@"Info of individual with ID: "+row.GEDCOM_ID);     
                        }
                    }
                    content_list.Add(temp_content); // Add the parsed data to the content list that will be displayed on the page

                    // We now add some nice functionality that will interpret the relationships that the individual has within the families he belongs to

                    query_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID) && (s.Tag.Equals("FAMS") || s.Tag.Equals("FAMC"))).ToList();
                    foreach (var fam in query_results)
                    {
                        string famID = fam.Data.Replace("@", "");
                        temp_content = "";
                        title_list.Add("Individual's position within family: <a href=\"Query?ID=" + famID + "&Tag=FAM\">" + famID + "</a>");
                        fam_query_results = search_result.Where(s => s.GEDCOM_ID.Equals(famID)).ToList(); // Find the relevant family

                        foreach (var row in fam_query_results)
                        {
                            switch (row.Tag) // Each relationship will be displayed in a different way
                            {
                                case "HUSB" :
                                {
                                    if (!row.Data.Equals(""))
                                    {
                                        string husbID = row.Data.Replace("@", "");
                                        temp_content += "The male spouse in this family is <a href=\"Query?ID=" + husbID + "&Tag=INDI\">" + GetName(husbID) + " (" + husbID + ")" + "</a>.\n";
                                    }
                                    break;
                                }
                                case "WIFE" :
                                {
                                    if (!row.Data.Equals(""))
                                    {
                                        string wifeID = row.Data.Replace("@", "");
                                        temp_content += "The female spouse in this family is <a href=\"Query?ID=" + wifeID + "&Tag=INDI\">" + GetName(wifeID) + " (" + wifeID + ")" + "</a>.\n";
                                    }
                                    break;
                                }
                                case "CHIL" :
                                {
                                    string childID = row.Data.Replace("@", "");
                                    string Date = FindDate("BIRT", childID); // Find the date that each child was born on
                                    temp_content += "<a href=\"Query?ID=" + childID + "&Tag=INDI\">" + GetName(childID) + " (" + childID + ")" + "</a> is a child in this family";
                                    if (Date != null) // If no date was provided in the GEDCOM file, sentance structure should be different
                                    {
                                        temp_content += " and was born on " + Date + ".\n";
                                    }
                                    else
                                    {
                                        temp_content += ".\n";
                                    }
                                    break;
                                }
                                case "MARR" :
                                {
                                    string Date = FindDate("MARR", famID); // Find the date of marriage
                                    temp_content += "The marriage between husband and wife of this family ocurred";
                                    if (Date != null) // If no date was provided in the GEDCOM file, sentance structure should be different
                                    {
                                        temp_content += " on " + Date + ".\n";
                                    }
                                    else
                                    {
                                        temp_content += " on an unknown date.\n";
                                    }
                                    break;
                                }
                                case "DIV" :
                                {
                                    if (row.Data.Equals(""))
                                    {
                                        string Date = FindDate("DIV", famID); // Find the date of divorce
                                        temp_content += "A divorce occurred between the spouses of this family";
                                        if (Date != null) // If no date was provided in the GEDCOM file, sentance structure should be different
                                        {
                                            temp_content += " on " + Date + ".\n";
                                        }
                                        else
                                        {
                                            temp_content += " on an unknown date.\n";
                                        }
                                    }
                                    else
                                    {
                                        if (row.Data.Equals("Y")) // royal.ged seems to use the divorce tag differently (adding either a Y or a N as data)
                                        {
                                            temp_content += "A divorce occurred between the spouses of this family.\n";
                                        }
                                    }
                                    break;
                                }
                                default :
                                {
                                    break;
                                }
                            }
                        }
                        content_list.Add(temp_content); // Add the parsed data to the content list that will be displayed on the page
                    }
                    break;
                }

                // For FAM data: families whose information is represented in the GEDCOM file
                case "FAM":
                {
                    query_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID)).ToList(); // Find all of the relevant entries in the DB
                    foreach (var row in query_results)
                    {
                        if (row.Level != 0) // We don't want to display the first entry as data --- we'll make that the title heading later
                        {
                            tag_expanded = db.Tag_Conv.Find(row.Tag).Tag_Expanded; // Translate the tag from GEDCOM jargon to English
                            temp_content += ("".PadRight(5 * row.Level) + tag_expanded + ":").PadRight(50) + row.Data + "\n"; // Indent the entry according to its level and add the data
                        }
                        else // Make the first entry (the one with level 0) the title
                        {
                            title_list.Add("Info of family with ID: " + row.GEDCOM_ID);
                        }
                    }
                    content_list.Add(temp_content); // Add the parsed data to the content list that will be displayed on the page

                    // We now add some nice functionality that will interpret all of the relationships that the family contains

                    string famID = ID;
                    temp_content = "";
                    title_list.Add("Elaborated details of family:");
                    fam_query_results = search_result.Where(s => s.GEDCOM_ID.Equals(famID)).ToList();

                    foreach (var row in fam_query_results) // Each relationship will be displayed in a different way
                    {
                        switch (row.Tag)
                        {
                            case "HUSB":
                                {
                                    if (!row.Data.Equals(""))
                                    {
                                        string husbID = row.Data.Replace("@", "");
                                        temp_content += "The male spouse in this family is <a href=\"Query?ID=" + husbID + "&Tag=INDI\">" + GetName(husbID) + " (" + husbID + ")" + "</a>.\n";
                                    }
                                    break;
                                }
                            case "WIFE":
                                {
                                    if (!row.Data.Equals(""))
                                    {
                                        string wifeID = row.Data.Replace("@", "");
                                        temp_content += "The female spouse in this family is <a href=\"Query?ID=" + wifeID + "&Tag=INDI\">" + GetName(wifeID) + " (" + wifeID + ")" + "</a>.\n";
                                    }
                                    break;
                                }
                            case "CHIL":
                                {
                                    string childID = row.Data.Replace("@", "");
                                    string Date = FindDate("BIRT", childID); // Find the date of each child's birthday
                                    temp_content += "<a href=\"Query?ID=" + childID + "&Tag=INDI\">" + GetName(childID) + " (" + childID + ")" + "</a> is a child in this family";
                                    if (Date != null) // If no date was provided in the GEDCOM file, sentance structure should be different
                                    {
                                        temp_content += " and was born on " + Date + ".\n";
                                    }
                                    else
                                    {
                                        temp_content += ".\n";
                                    }
                                    break;
                                }
                            case "MARR":
                                {
                                    string Date = FindDate("MARR", famID); // Find the date of marriage
                                    temp_content += "The marriage between husband and wife of this family ocurred";
                                    if (Date != null) // If no date was provided in the GEDCOM file, sentance structure should be different
                                    {
                                        temp_content += " on " + Date + ".\n";
                                    }
                                    else
                                    {
                                        temp_content += " on an unknown date.\n";
                                    }
                                    break;
                                }
                            case "DIV":
                                {
                                    if (row.Data.Equals(""))
                                    {
                                        string Date = FindDate("DIV", famID); // Find the date of divorce
                                        temp_content += "A divorce occurred between the spouses of this family";
                                        if (Date != null) // If no date was provided in the GEDCOM file, sentance structure should be different
                                        {
                                            temp_content += " on " + Date + ".\n";
                                        }
                                        else
                                        {
                                            temp_content += " on an unknown date.\n";
                                        }
                                    }
                                    else
                                    {
                                        if (row.Data.Equals("Y")) // royal.ged seems to use the divorce tag differently (adding either a Y or a N as data)
                                        {
                                            temp_content += "A divorce occurred between the spouses of this family.\n";
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    content_list.Add(temp_content); // Add the parsed data to the content list that will be displayed on the page

                    break;
                }

                default:
                {
                    break;
                }
            }
            // Add everthing we've done to the Heading and Content ViewBags (utilized by the View to display the info)
            ViewBag.Heading = title_list;
            ViewBag.Content = content_list;
            return View();
        }

        // A function to return the date of any event as specified by the tag and GEDCOM ID
        private string FindDate(string tag, string ID)
        {
            var search_result = from entries in db.GEDCOM_Data select entries;
            int level = -1; // Initialise a variable to keep track of which date we want (it must always be one level down from that of the event)
            List<GEDCOM_Data> temp_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID)).ToList();
            foreach (var temp_row in temp_results)
            {
                if (temp_row.Level <= level) // The date's level can never be higher that that of the event
                {
                    return null;
                }
                if (temp_row.Tag.Equals(tag))
                {
                    level = temp_row.Level; // Set the current level of the event entry...
                }
                else
                {
                    if (temp_row.Tag.Equals("DATE")) // ... and find it's date
                    {
                        return temp_row.Data;
                    }
                }
            }
            return null;            
        }

        // A function to get the name of an individual based on his GEDCOM ID
        private string GetName(string ID)
        {
            var search_result = from entries in db.GEDCOM_Data select entries;
            List<GEDCOM_Data> temp_results = search_result.Where(s => s.GEDCOM_ID.Equals(ID)).ToList();
            foreach (var temp_row in temp_results)
            {
                if (temp_row.Tag.Equals("NAME")) //  Fairly straight forward
                {
                    return temp_row.Data;
                }
            }
            return null;
        }

    }
}
