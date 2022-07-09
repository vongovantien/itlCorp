using System;
using System.Collections.Generic;
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
        public List<ServiceOffice> ServiceOffice { get; set; }
    }

    public class ServiceOffice
    {
        public string Service { get; set; }
        public string Office { get; set; }
    }
}
