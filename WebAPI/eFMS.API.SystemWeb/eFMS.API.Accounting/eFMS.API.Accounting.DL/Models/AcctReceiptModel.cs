using eFMS.API.Accounting.DL.Models.Receipt;
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
        public bool IsReceiptBankFee { get; set; } 
        public string ReceiptInternalOfficeCode { get; set; }
        public string ObhPartnerName { get; set; }
        public string ContractNo { get; set; }
    }

    public enum SaveAction
    {
        SAVEDRAFT_ADD = 0,
        SAVEDRAFT_UPDATE = 1,
        SAVEDONE = 2,
        SAVECANCEL = 3,
        SAVEBANK_ADD = 4,
        SAVEBANK_DONE = 5
    }
}
