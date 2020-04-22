﻿using System;

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
        public string SubAbbr { get; set; }
        public DateTime? DeliveryOrderPrintedDate { get; set; }
    }
}
