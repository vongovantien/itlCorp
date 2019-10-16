using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class TariffCriteria
    {
        public string Name { get; set; }
        public string ServiceMode { get; set; }
        public string CustomerID { get; set; }
        public string SupplierID { get; set; }
        public string TariffType { get; set; }
        public string DateType { get; set; }
        public DateTime? Date { get; set; }

        public bool? Status { get; set; }
        public string OfficeId { get; set; }
    }
}
