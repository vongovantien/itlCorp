using System;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class AccountReceivableMoveDataSalesman
    {
        [Required]
        public string PartnerId { get; set; }
        [Required]
        public Guid ContractId { get; set; }
        [Required]
        public string FromSalesman { get; set; }
        [Required]
        public string ToSalesman { get; set; }
    }
}
