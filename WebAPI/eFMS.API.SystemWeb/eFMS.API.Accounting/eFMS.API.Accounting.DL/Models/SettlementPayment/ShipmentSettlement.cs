﻿using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class ShipmentSettlement
    {
        public string SettlementNo { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyShipment { get; set; }
        public List<ShipmentChargeSettlement> ChargeSettlements { get; set; }
    }
}
