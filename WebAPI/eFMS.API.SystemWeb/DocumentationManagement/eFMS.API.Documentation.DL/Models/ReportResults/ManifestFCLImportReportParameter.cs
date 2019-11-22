using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class ManifestFCLImportReportParameter
    {
        public string SFrom { get; set; }
        public string SumCarton { get; set; }
        public string No { get; set; }
        public string MBL { get; set; }
        public string ShippingMark { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public int DecimalNo { get; set; }
    }
}
