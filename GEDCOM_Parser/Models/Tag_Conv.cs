/* 
 * Model for the Tag Conversion Database. The purpose of this database is twofold:
 * 1) To display the GEDCOM tag information in plain english for the user to understand.
 * 2) To validate each GEDCOM tag in the file upload. Unsupported tags are not allowed.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GEDCOM_Parser.Models
{
    public class Tag_Conv
    {
        public string ID { get; set; } // The actual GEDCOM tag is the primary key
        public string Tag_Expanded { get; set; } // The corresponding plain english term
    }
}