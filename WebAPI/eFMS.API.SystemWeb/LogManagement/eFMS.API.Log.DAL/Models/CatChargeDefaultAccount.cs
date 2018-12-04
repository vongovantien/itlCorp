using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Models
{
    public class CatChargeDefaultAccount
    {
        public Guid Id { get; set; }
        public PropertyCommon PropertyCommon { get; set; }
        public CatChargeDefaultAccountEntity NewObject { get; set; }
    }
    public class CatChargeDefaultAccountEntity
    {
        public int Id { get; set; }
        public Guid ChargeId { get; set; }
        public string DebitAccountNo { get; set; }
        public decimal? DebitVat { get; set; }
        public string CreditAccountNo { get; set; }
        public decimal? CreditVat { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
