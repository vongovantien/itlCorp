using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class CsBookingNoteCriteria
    {
        public string All { get; set; }
        public string BookingNo { get; set; }
        public string ShipperName { get; set; }
        public string ConsigneeName { get; set; }
        public string POLName { get; set; }
        public string PODName { get; set; }
        public string CreatorName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
}
