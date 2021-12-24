using eFMS.API.ForPartner.DL.Anotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.ForPartner.DL.Models
{
    public class VoucherSyncCreateModel
    {
        [Required]
        public Guid DocID { get; set; }
        [Required]
        public string DocCode { get; set; }
        [StringContainAttribute(AllowableValues = new string[] {
            "CDNOTE",
            "SETTLEMENT",
            "SOA"
        })]
        public string DocType { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        [Required]
        public string OfficeCode { get; set; }
        public string PaymentMethod { get; set; }

        public List<VoucherCreateRowModel> Details { get; set; }
    }

    public class VoucherCreateRowModel
    {
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        public Guid ChargeId { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal AmountVnd { get; set; }
        public decimal AmountUsd { get; set; }
        public decimal VatAmountVnd { get; set; }
        public decimal VatAmountUsd { get; set; }
        public int PaymentTerm { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SerieNo { get; set; }
        public string AccountNo { get; set; }
        public string VoucherType { get; set; }
        [StringContainAttribute(AllowableValues = new string[] {
            "CREDIT",
            "ADV",
            "OBH",
            "NONE"
        })]
        public string TransactionType { get; set; }
        public string JobNo { get; set; }
        public string MblNo { get; set; }
        public string Hblno { get; set; }
        public string BravoRefNo { get; set; }
    }

    public class VoucherSyncUpdateModel
    {
        public Guid DocID { get; set; }
        [Required]
        public string VoucherNo { get; set; }
        [Required]
        public string CustomerCode { get; set; }
        public List<VoucherSyncUpdatRowModel> Details { get; set; }
    }

    public class VoucherSyncUpdatRowModel
    {
        public Guid ChargeID { get; set; }
        [StringContainAttribute(AllowableValues = new string[] {
            "CREDIT",
            "ADV",
            "OBH",
            "NONE"
        })]
        public string TransactionType { get; set; }
        public string AccountNo { get; set; }
        public string BravoRefNo { get; set; }
    }

    public class VoucherSyncDeleteModel
    {
        public string VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
        [Required]
        public string DocCode { get; set; }
        [StringContainAttribute(AllowableValues = new string[] {
            "CDNOTE",
            "SETTLEMENT",
            "SOA"
        })]
        public string DocType { get; set; }
        public string OfficeCode { get; set; }
    }

    public class VoucherGroupDetail
    {
        public string VoucherNo { get; set; }
        public string TransactionType { get; set; }
        public VoucherCreateRowModel VoucherData { get; set; }
        public List<object> Surcharges { get; set; }
    }

 
}
 