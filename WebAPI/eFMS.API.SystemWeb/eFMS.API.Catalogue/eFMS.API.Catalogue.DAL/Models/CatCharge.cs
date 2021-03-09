using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatCharge
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string ChargeNameVn { get; set; }
        public string ChargeNameEn { get; set; }
        public string ServiceTypeId { get; set; }
        public string Type { get; set; }
        public string CurrencyId { get; set; }
        public decimal UnitPrice { get; set; }
        public short UnitId { get; set; }
        public decimal Vatrate { get; set; }
        public bool? IncludedVat { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public Guid? DebitCharge { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? ChargeGroup { get; set; }
        public string ProductDept { get; set; }
        public string Mode { get; set; }
    }
}
