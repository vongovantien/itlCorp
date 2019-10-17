using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class TariffViewModel
    {
        public Guid Id { get; set; }
        public string TariffName { get; set; }
        public string TariffType { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public Guid OfficeID { get; set; }
        public string ServiceMode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public bool? Status { get; set; }

    }
}
