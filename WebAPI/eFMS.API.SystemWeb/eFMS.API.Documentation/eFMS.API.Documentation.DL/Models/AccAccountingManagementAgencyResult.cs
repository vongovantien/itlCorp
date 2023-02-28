using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class AccAccountingManagementAgencyResult
    {
        public string InvoiceNo { get; set; }
        public string JobNo { get; set; }
        public object Type { get; set; }
        public DateTime? IssueDate { get; set; }
        public string FlexId { get; set; }
        public string MAWB { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public decimal? ChargeWeight { get; set; }
        public string Comment { get; set; }
        public string Hbl { get; set; }
        public string CdNoteNo { get; set; }
        public string CdNoteType { get; set; }
        public string ChargeType { get; set; }
        public string Currency { get; set; }
        public decimal? OriginChargeAmount { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string SoaNo { get; set; }
        public string Status { get; set; }
        public string CodeType { get; set; }
        public decimal? FreightAmount { get; set; }
        public decimal? DebitUsd { get; set; }
        public decimal? CreditUsd { get; set; }
        public decimal? Balance { get; set; }
        public string INVCreNo { get; set; }
        public string VatVoucher { get; set; }
        public DateTime? InvDueDay { get; set; }
        public string SoaSmNo { get; internal set; }
    }
}
