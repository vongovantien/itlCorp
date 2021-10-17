using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SetTariff
    {
        public Guid Id { get; set; }
        public string TariffName { get; set; }
        public string TariffType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid ApplyOfficeId { get; set; }
        public string ProductService { get; set; }
        public string CargoType { get; set; }
        public string ServiceMode { get; set; }
        public string CustomerId { get; set; }
        public string SupplierId { get; set; }
        public string AgentId { get; set; }
        public string Description { get; set; }
        public bool? Status { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? OfficeId { get; set; }
    }
}
