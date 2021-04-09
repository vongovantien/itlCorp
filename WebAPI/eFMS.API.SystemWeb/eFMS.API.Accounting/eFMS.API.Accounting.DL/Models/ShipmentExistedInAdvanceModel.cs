using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class ShipmentExistedInAdvanceModel
    {
        public string AdvanceNo { get; set; }
        public string Currency { get; set; }
        public string Requester { get; set; }
        public DateTime? RequestDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string StatusApproval { get; set; }
    }
}
