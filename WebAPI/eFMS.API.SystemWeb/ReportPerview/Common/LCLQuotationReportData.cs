using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportPerview.Common
{
    public class LCLQuotationReportData
    {
        public string ShortName { get; set; }
        public string PartnerName_VN { get; set; }
        public string PartnerName_EN { get; set; }
        public DateTime DatetimeCreated { get; set; }
        public string ContactName_VN { get; set; }
        public string ContactName_EN { get; set; }
        public string JobTitle { get; set; }
        public string CommodityName_VN { get; set; }
        public string CommodityName_EN { get; set; }
        public string PickupAdress { get; set; }
        public string DeliveryAddress { get; set; }
        public string EmployeeName_VN { get; set; }
        public string SalePosition { get; set; }
        public string HomePhone { get; set; }
        public string ExtNo { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public int MaximumDelayTime { get; set; }
        public string MaximumDelayTimeUnit { get; set; }
        public int PaymentDeadline { get; set; }
        public string PaymentDeadlineUnit { get; set; }
        public DateTime EffectiveOn { get; set; }
        public DateTime ExpiryOn { get; set; }
        public decimal FuelPrice { get; set; }
    }
}