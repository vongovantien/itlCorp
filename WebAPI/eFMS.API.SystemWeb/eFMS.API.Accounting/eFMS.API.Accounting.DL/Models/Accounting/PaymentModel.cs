using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class PaymentModel
    {
        public string Stt { get; set; }
        public string BranchCode { get; set; }
        public string OfficeCode { get; set; }
        public string Transcode { get; set; }
        public DateTime? DocDate { get; set; }
        public string ReferenceNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Description0 { get; set; }
        public string PaymentMethod { get; set; }
        public string DataType { get; set; }
        public List<PaymentDetailModel> Details { get; set; }
    }

    public class PaymentDetailModel
    {
        public string RowId { get; set; }
        public decimal? OriginalAmount { get; set; }
        public string Description { get; set; }
        public string ObhPartnerCode { get; set; }
        public string BankAccountNo { get; set; }
        public string Stt_Cd_Htt { get; set; }
        public string ChargeType { get; set; }
        public string DebitAccount { get; set; }
    }
}
