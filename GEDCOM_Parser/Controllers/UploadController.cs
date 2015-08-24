/* 
 * Controller for the UPLOAD page. Allows the user to upload a file; which it then reads, validates
 * and finally stores into the database. A state machine with 5 states is used to upload the data
 * into the DB in an intelligent way (getting rid of any whitespace, line breaks and other hazards
 * that might choke up the parsing process later).
 * Note that all file validation checks (both external and internal) happen in this controller.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GEDCOM_Parser.Models;
using System.Data.Entity.Infrastructure;

namespace GEDCOM_Parser.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/

        public static int validFile = 1; // A flag which will be used to validate the files that a user uploads
        private GedDBContext db = new GedDBContext(); // The variable that gives us access to our database

        public ActionResult Index()
        {
            switch (validFile) // Let's inform the user on what we think of his file
            {
                case 1:
                    ViewBag.statusLabel = "";
                    break;      
                case 2:
                    ViewBag.statusLabel = "That file is empty, unfortunately.";
                    break;            
                case 3:
                    ViewBag.statusLabel = "That's not a .ged file!";
                    break;
                case 4:
                    ViewBag.statusLabel = "This file is too big, sorry (max 5MB).";
                    break;
                case 5:
                    ViewBag.statusLabel = "Your GEDCOM file is broken (or has unsupported tags). Please check for formatting issues and re-upload.";
                    break;
                default:
                    ViewBag.statusLabel = "";
                    break;
            }
            return View();
        }

        // Upload the file (if it is valid)
        [HttpPost]
        public ActionResult UploadAction(HttpPostedFileBase ged_file)
        {
            // Check all the attributes of the uploaded file and set any validation flags along the way
            if (ged_file != null && ged_file.ContentLength > 0)
            {
                if (System.IO.Path.GetExtension(ged_file.FileName).ToLower() == ".ged")
                {
                    if (ged_file.ContentLength < 5 * 1024 * 1024)
                    {
                        if (GED_to_DB(ged_file))
                        {
                            validFile = 1; // A valid file
                            return RedirectToAction("Index","Display");
                        }
                        else
                        {
                            validFile = 5; // It is a GEDCOM file, but it is broken (i.e.: unsupported tags)
                        }
                    }
                    else
                    {
                        validFile = 4; // It is a GEDCOM file, but it is way to big
                    }
                }
                else
                {
                    validFile = 3; // It is not a GEDCOM file to begin with
                }
            }
            else
            {
                validFile = 2; // You didn't upload anything
            }
            return RedirectToAction("Index");
        }

        // Read the .ged file and store its contents in the database
        public Boolean GED_to_DB(HttpPostedFileBase file)
        {
            // Initialise all flags, variables, counters and input streams
            int state = 1;
            int ID_counter = 0;
            Boolean zero_flag = true;
            Boolean space_flag = false;
            string ID_string = "";
            string tag_string = "";
            string data_string = "";
            int file_len = file.ContentLength;
            byte[] ged_data = new byte[file_len];
            file.InputStream.Read(ged_data,0,file_len);
            var data_temp = System.Text.Encoding.ASCII.GetString(ged_data);

            // Gain access to our database and clear all of its contents before we begin
            GEDCOM_Data gedcom_DB = db.GEDCOM_Data.Create();
            db.Database.ExecuteSqlCommand("DELETE FROM GEDCOM_Data");

            foreach(char current_char in data_temp)
            {
                // Create the state machine
                switch (state)
                {
                    // State 1: Obtain the GEDCOM entry's level
                    case 1:
                        if (!(current_char.Equals('\n') || current_char.Equals('\r'))) // Ignore any empty lines
                        {
                            gedcom_DB.Level = (int)current_char - (int)'0'; // Obtain the level
                            if (gedcom_DB.Level > 10 && gedcom_DB.Level <= 0) // Perform some sanity checks on the level
                            {
                                return false;
                            }
                            if (gedcom_DB.Level == 0) // We will handle level 0 data differently in succeeding states
                            {
                                zero_flag = true;
                                ID_string = "";
                            }
                            else
                            {
                                zero_flag = false;
                            }
                            space_flag = false; // We haven't had to handle any spaces yet, set the flag up for this
                            state = 2; // Proceed to the next state
                        }
                        break;
                    // State 2: Handle any unwanted white spaces between the level and the tag/ID
                    case 2:
                        if (!current_char.Equals(' ')) // Get rid of all the white space
                        {
                            if (!space_flag)
                            {
                                return false;
                            }
                            if (!zero_flag) // If we aren't handeling a 0 level entry...
                            {
                                tag_string += current_char; // ... we build up the first piece of the tag
                            }
                            else // If we are handeling a 0 level entry...
                            {
                                ID_string += current_char; // ... we build up the first piece of the ID
                            }
                            state = 3; // Proceed to the next state
                        }
                        space_flag = true;
                        break;
                    // State 3: Build up the ID (for level 0 entries) or the tag (for other level entries)
                    case 3:
                        if (!current_char.Equals(' ') && !(current_char.Equals('\n') || current_char.Equals('\r')))
                        {
                            if (!zero_flag) // If we aren't handeling a 0 level entry...
                            {
                                tag_string += current_char; // ... we build up the rest of the tag
                            }
                            else // If we are handeling a 0 level entry...
                            {
                                ID_string += current_char; // ... we build up the rest of the ID
                            }
                        }
                        else // Once we've finished building up the tag/ID
                        {
                            if (zero_flag) // For level zero entries, format and save the ID data
                            {
                                ID_string = ID_string.Replace("@", "");
                                gedcom_DB.GEDCOM_ID = ID_string;
                                gedcom_DB.Tag = ID_string;
                            }
                            else // For all other level entries, save the tag data
                            {
                                gedcom_DB.Tag = tag_string;
                                tag_string = "";
                            }
                            if (!current_char.Equals(' '))
                            {
                                // If there is no GEDCOM DATA (text) in the entry, set everything up to save the data into the database
                                gedcom_DB.GEDCOM_ID = ID_string;
                                gedcom_DB.Data = "";
                                state = 1;
                                tag_string = "";
                                ID_counter++;
                                gedcom_DB.ID = ID_counter;
                                db.GEDCOM_Data.Add(gedcom_DB);
                                if (db.Tag_Conv.Find(gedcom_DB.Tag) == null) // Check if the tag of the entry that is going to be saved is valid
                                {
                                    return false;
                                }
                                try // Save the data we have just formulated into the database
                                {
                                    db.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                    return false;
                                }
                                gedcom_DB = db.GEDCOM_Data.Create(); // Reset
                                break;
                            }
                            state = 4; // Proceed to the next state
                        }
                        break;
                    // State 4: Once again, handle unneccessary white space
                    case 4:
                        if (!current_char.Equals(' '))
                        {
                            if (zero_flag) // For level zero entries...
                            {
                                tag_string += current_char; // ... we build up the first piece of the tag
                            }
                            else // For other level entries...
                            {
                                data_string += current_char; // ... we build up the first piece of the data
                            }
                            state = 5; // Proceed to the next state
                        }
                        break;
                    // State 5: Finish building the tag/data (dependant on level)
                    case 5:
                        if (!(current_char.Equals('\n') || current_char.Equals('\r'))) // Check if we haven't reached the end of the line entry yet
                        {
                            if (zero_flag)
                            {
                                if (current_char.Equals(' ')) // In the rare case that an entry has a level, ID, tag AND data, we handle this correctly
                                {
                                    state = 4;
                                    zero_flag = false;
                                    gedcom_DB.Tag = tag_string;
                                }
                                else // We build up the rest of the tag (level 0 entries)
                                {
                                    tag_string += current_char;
                                }
                            }
                            else // We build up the rest of the data (other level entries)
                            {
                                data_string += current_char;
                            }
                        }
                        else // Once we've reached the end of the line entry...
                        {
                            if (zero_flag) // ... save the tag for level 0 entries
                            {
                                gedcom_DB.Tag = tag_string;
                                gedcom_DB.Data = "";
                            }
                            else // ... save the data for level 0 entries
                            {
                                gedcom_DB.Data = data_string;
                                gedcom_DB.GEDCOM_ID = ID_string;
                            }

                            // Set everything up to save the entry into the database
                            data_string = "";
                            tag_string = "";
                            state = 1;
                            ID_counter++;
                            gedcom_DB.ID = ID_counter;
                            db.GEDCOM_Data.Add(gedcom_DB);
                            if (db.Tag_Conv.Find(gedcom_DB.Tag) == null) // Check if the tag of the entry that is going to be saved is valid
                            {
                                return false;
                            }
                            try // Save the data we have just formulated into the database
                            {
                                db.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                return false;
                            }
                            gedcom_DB = db.GEDCOM_Data.Create(); // Reset
                        }
                        break;
                    default:
                        break;
                }
            }
            return true; // We managed to read the entire file successfully!
        }
    }
}
