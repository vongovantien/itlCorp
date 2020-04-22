﻿using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults.Sales
{
    public class QuaterSaleReportParameter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Contact { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public decimal? CurrDecimalNo { get; set; }
        public string ReportBy { get; set; }
        public string SalesManager { get; set; }
        public string Director { get; set; }
        public string ChiefAccountant { get; set; }
    }
}
