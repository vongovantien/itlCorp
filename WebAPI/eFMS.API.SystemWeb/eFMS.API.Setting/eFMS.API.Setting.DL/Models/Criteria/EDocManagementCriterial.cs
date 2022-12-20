using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class EDocManagementCriterial
    {
        public string ReferenceNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateMode? DateMode { get; set; }
        public List<AccountantType> AccountantTypes { get; set; }
        public ReferenceType? ReferenceType { get; set; }
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 15;
        public bool isAcc { get; set; }
    }
    public enum ReferenceType
    {
        JobId,
        HouseBill,
        MasterBill,
        AccountantNo,
        InvoiceNo
    }
    public enum AccountantType
    {
        CashReceipt,
        CashPayment,
        DebitSlip,
        CreditSlip,
        PurchasingNote,
        OtherEntry
    }
    public enum DateMode
    {
        CreateDate,
        AccountingDate,
    }
}
