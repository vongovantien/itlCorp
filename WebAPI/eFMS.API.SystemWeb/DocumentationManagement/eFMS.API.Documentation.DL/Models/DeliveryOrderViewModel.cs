using eFMS.API.Documentation.DL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class DeliveryOrderViewModel
    {
        public Guid HBLID { get; set; }
        public string DeliveryOrderNo { get; set; }
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public string Doheader1 { get; set; }
        public string Doheader2 { get; set; }
        public string Dofooter { get; set; }
        public DateTime? DeliveryOrderPrintedDate { get; set; }
    }
}
