using eFMS.API.ForPartner.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ForPartner.DL.ViewModel
{
    public class ChargeInvoiceUpdateTable
    {
        public Guid? Id { get; set; }
        public Guid? AcctManagementId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string VoucherId { get; set; }
        public DateTime? VoucherIddate { get; set; }
        public string SeriesNo { get; set; }
        public decimal? FinalExchangeRate { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? Total { get; set; }
        public decimal? AmountVnd { get; set; }
        public decimal? VatAmountVnd { get; set; }
        public decimal? AmountUsd { get; set; }
        public decimal? VatAmountUsd { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
