/* 
 * Model for the GEDCOM Data Database. The purpose of this database is to store the data
 * uploaded from the user's file. This allows multiple queries and searches to be performed
 * on the data throughout the duration of the user's time on the web app.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GEDCOM_Parser.Models
{
    public class GEDCOM_Data
    {
        public int ID { get; set; } // An integer used to store the number of the entry (the primary key)
        public int Level { get; set; } // The GEDCOM level of the entry - can range from 0 to 9
        public string GEDCOM_ID { get; set; } // The GEDCOM ID of the entry - all child entries without explicit IDs are given the IDs of their parents
        public string Tag { get; set; } // The GEDCOM tag - which specifies what type of information the entry contains
        public string Data { get; set; } // The actual GEDCOM data for the entry
    }
}