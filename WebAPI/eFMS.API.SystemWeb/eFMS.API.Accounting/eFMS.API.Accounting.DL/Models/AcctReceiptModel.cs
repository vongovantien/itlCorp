﻿using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models
{
    public class AcctReceiptModel: AcctReceipt
    {
        public string CustomerName { get; set; }
        public string TaxCode { get; set; }
        public List<ReceiptInvoiceModel> Payments { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string SubRejectReceipt { get; set; }
        public bool IsReceiptBankFee { get; set; } // cho phép tạo phiếu thu ngân hàng.
    }

    public enum SaveAction
    {
        SAVEDRAFT_ADD = 0,
        SAVEDRAFT_UPDATE = 1,
        SAVEDONE = 2,
        SAVECANCEL = 3
    }
}
