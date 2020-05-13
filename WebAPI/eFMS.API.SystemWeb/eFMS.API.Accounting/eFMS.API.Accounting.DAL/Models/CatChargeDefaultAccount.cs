using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class CatChargeDefaultAccount
    {
        public int Id { get; set; }
        public Guid ChargeId { get; set; }
        public string DebitAccountNo { get; set; }
        public string DebitVat { get; set; }
        public string CreditAccountNo { get; set; }
        public string CreditVat { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
