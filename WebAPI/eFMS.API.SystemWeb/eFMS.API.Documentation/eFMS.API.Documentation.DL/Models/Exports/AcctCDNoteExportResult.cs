using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Exports
{
    public class AcctCDNoteExportResult
    {
        public List<ExportCDNoteModel> ListCharges { get; set; }
        public string CDNo { get; set; }
        public string PartnerNameEn { get; set; }
        public string BillingAddress { get; set; }
        public string Taxcode { get; set; }
        public string ClearanceNo { get; set; }
        public decimal? GW { get; set; }
        public decimal? CBM { get; set; }
        public string HBL { get; set; }
        public string Cont { get; set; }
        public string WareHouseName { get; set; }
        public decimal? TotalFeeOBH { get; set; }
        public decimal? TotalVATOBH { get; set; }
        public decimal? TotalOBH { get; set; }

        public string BankAccountVND { get; set; }
        public string OfficeEn { get; set; }
        public string BankNameEn { get; set; }
        public string BankAddressEn { get; set; }
        public string BankAccountNameEn { get; set; }
        public string SwiftCode { get; set; }

    }
}