using eFMS.API.SystemFileManagement.DL.Models.Receipt;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.SystemFileManagement.DL.Models.Criteria
{
    public class ProcessReceiptInvoice
    {
        [Required]
        public decimal PaidAmountVnd { get; set; }
        public decimal PaidAmountUsd { get; set; }
        public List<ReceiptInvoiceModel> List { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public decimal FinalExchangeRate { get; set; }
        
    }
}
