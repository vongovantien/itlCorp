using eFMS.API.Setting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class SetTariffDetailModel : SetTariffDetail
    {
        public string ChargeName { get; set; }
        public string ChargeCode { get; set; }
        public string CommodityName { get; set; }
        public string PayerName { get; set; }
        public string PortName { get; set; }
        public string WarehouseName { get; set; }
    }
}
