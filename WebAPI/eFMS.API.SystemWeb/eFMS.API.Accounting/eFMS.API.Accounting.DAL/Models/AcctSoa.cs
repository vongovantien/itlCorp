using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AcctSoa
    {
        public int Id { get; set; }
        public string Soano { get; set; }
        public DateTime? SoaformDate { get; set; }
        public DateTime? SoatoDate { get; set; }
        public string DateType { get; set; }
        public string Currency { get; set; }
        public int? TotalShipment { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? DebitAmount { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string Customer { get; set; }
        public string Type { get; set; }
        public bool? Obh { get; set; }
        public string CreatorShipment { get; set; }
        public string ServiceTypeId { get; set; }
        public short? CommodityGroupId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string PaymentStatus { get; set; }
        public int? PaymentExtendDays { get; set; }
        public string PaymentNote { get; set; }
        public DateTime? PaymentDatetimeUpdated { get; set; }
        public DateTime? LastSyncDate { get; set; }
    }
}
