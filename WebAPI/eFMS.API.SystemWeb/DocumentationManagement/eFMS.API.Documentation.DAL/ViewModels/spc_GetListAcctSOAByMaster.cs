using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class spc_GetListAcctSOAByMaster
    {
        public int ID { get; set; }
        public string SOANo { get; set; }
        public string PartnerName { get; set; }
        public int Shipment { get; set; }
        public string Currency { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public string Status { get; set; }
        public DateTime DatetimeCreated { get; set; }
        public string UserCreated { get; set; }
        public DateTime DatetimeModified { get; set; }
        public string UserModified { get; set; } 
        public string Note { get; set; }
        public DateTime? SoaformDate { get; set; }
        public DateTime? SoatoDate { get; set; }
    }
}
