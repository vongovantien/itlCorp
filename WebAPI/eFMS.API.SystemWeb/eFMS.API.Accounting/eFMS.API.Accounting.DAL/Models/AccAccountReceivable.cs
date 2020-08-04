using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AccAccountReceivable
    {
        public Guid Id { get; set; }
        public string AcRef { get; set; }
        public string PartnerId { get; set; }
        public Guid? Office { get; set; }
        public string Service { get; set; }
        public string SaleMan { get; set; }
        public Guid? ContractId { get; set; }
        public string ContractCurrency { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? BillingAmount { get; set; }
        public decimal? BillingUnpaid { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? ObhAmount { get; set; }
        public decimal? ObhUnpaid { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public decimal? SellingNoVat { get; set; }
        public decimal? Over1To15Day { get; set; }
        public decimal? Over16To30Day { get; set; }
        public decimal? Over30Day { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
