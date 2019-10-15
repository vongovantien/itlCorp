using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class TariffModel
    {
        public Guid Id { get; set; }
        public string TariffName  { get; set; }
        public Guid OfficeID { get; set; }
        public string ProductService { get; set; }
        public string CargoType { get; set; }
        public string ServiceMode { get; set; }
        public string CustomerID { get; set; }
        public string SupplierID { get; set; }
        public string AgentID { get; set; }
        public string Description { get; set; }
        public bool? Status { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
