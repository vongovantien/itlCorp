using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Models.ExportResults
{
    public class AcctReceiptAdvanceModelExport
    {
        public string TaxCode { get; set; }
        public string PartnerNameEn { get; set; }
        public string PartnerNameVn { get; set; }
        public string UserExport { get; set; }
        public IQueryable<AcctReceiptAdvanceRowModel> Details { get; set; }
    }

    public class AcctReceiptAdvanceRowModel
    {
        public string ReceiptNo { get; set; }
        public string Description { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal TotalAdvancePaymentVnd { get; set; }
        public decimal TotalAdvancePaymentUsd { get; set; }
        public decimal CusAdvanceAmountVnd { get; set; }
        public decimal CusAdvanceAmountUsd { get; set; }
        public decimal AgreementCusAdvanceVnd { get; set; }
        public decimal AgreementCusAdvanceUsd { get; set; }


    }
}
