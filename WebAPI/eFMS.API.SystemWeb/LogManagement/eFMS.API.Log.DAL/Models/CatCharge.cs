using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Models
{
    public class CatCharge
    {
        public Guid Id { get; set; }
        public PropertyCommon PropertyCommon { get; set; }
        public CatChargeEntity NewObject { get; set; }
    }
    public class CatChargeEntity
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string ChargeNameVn { get; set; }
        public string ChargeNameEn { get; set; }
        public string ServiceTypeId { get; set; }
        public string Type { get; set; }
        public string CurrencyId { get; set; }
        public double UnitPrice { get; set; }
        public short UnitId { get; set; }
        public double Vat { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
