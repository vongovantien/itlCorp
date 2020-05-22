using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsBookingNoteModel : CsBookingNote
    {
        public string ShipperName { get; set; }
        public string ConsigneeName { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public string CreatorName { get; set; }
        public string ModifiedName { get; set; }


    }
}
