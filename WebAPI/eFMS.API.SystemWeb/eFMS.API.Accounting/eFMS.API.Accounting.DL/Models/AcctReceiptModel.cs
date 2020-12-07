using eFMS.API.Accounting.Service.Models;
namespace eFMS.API.Accounting.DL.Models
{
    public class AcctReceiptModel: AcctReceipt
    {
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        public string CustomerName { get; set; }

    }
}
