﻿using System;

namespace eFMS.API.SystemFileManagement.Service.ViewModels
{
    public class sp_GetShipmentDataInCombineBilling
    {
        public string Refno { get; set; }
        public Guid HblId { get; set; }
        public string JobNo { get; set; }
        public string Mblno { get; set; }
        public string Hwbno { get; set; }
        public string CustomNo { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
    }
}
