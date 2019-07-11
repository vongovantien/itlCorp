using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class FreightManifest
    {
        public string FieldKeyID { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string Curr { get; set; }
        public decimal VAT { get; set; }
        public decimal TotalValue { get; set; }
        public bool Dbt { get; set; }
        public bool Collect { get; set; }
        public string AccountNo { get; set; }
        public decimal DecimalNo { get; set; }
        public decimal CurrDecimalNo { get; set; }
    }
}
