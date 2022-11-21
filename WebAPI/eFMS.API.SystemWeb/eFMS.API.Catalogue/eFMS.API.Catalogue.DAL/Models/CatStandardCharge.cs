using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatStandardCharge
    {
        public Guid Id { get; set; }
        public Guid? ChargeId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Currency { get; set; }
        public decimal? VatRate { get; set; }
        public string Type { get; set; }
        public string TransactionType { get; set; }
        public string Service { get; set; }
        public string ServiceType { get; set; }
        public string Office { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
