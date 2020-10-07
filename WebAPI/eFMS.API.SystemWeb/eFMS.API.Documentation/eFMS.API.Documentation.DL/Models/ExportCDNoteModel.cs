using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ExportCDNoteModel
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public string Notes { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string VATInvoiceNo { get; set; }
    }
}
