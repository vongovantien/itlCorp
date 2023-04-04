using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatCharge
    {
        public string Code { get; set; }
        public string ChargeNameEn { get; set; }
        public string ChargeNameVn { get; set; }
        public string Type { get; set; }
        public bool? Active { get; set; }
        public string ServiceTypeId { get; set; }
        public string OfficesName { get; set; }
        public string ChargeGroupName { get; set; }
        public string BuyingCode { get; set; }
        public string BuyingName { get; set; }
        public string SellingCode { get; set; }
        public string SellingName { get; set; }
    }
}
