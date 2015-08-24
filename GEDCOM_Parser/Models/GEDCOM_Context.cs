/* 
 * Model for the database context. This model creates tables for both databases:
 * GEDCOM Data and Tag Conversion.
 * Developed by Jason Scott and Tiaan Naude, under TapX (Pty) Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace GEDCOM_Parser.Models
{
    public class GedDBContext : DbContext
    {
        public DbSet<GEDCOM_Data> GEDCOM_Data { get; set; }
        public DbSet<Tag_Conv> Tag_Conv { get; set; }
    }
}