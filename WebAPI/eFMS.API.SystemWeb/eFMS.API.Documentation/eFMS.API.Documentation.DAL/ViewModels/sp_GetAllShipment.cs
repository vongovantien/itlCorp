using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetAllShipment
    {
        public Guid Id { get; set; }
        public string JobNo { get; set; }
        public string MBLNo { get; set; }
        public string HWBNo { get; set; }
        public string ProductService { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string PersonInCharge { get; set; }
        public string SaleMan { get; set; }
        public string ServiceDate { get; set; }
        public string DatetimeCreated { get; set; }
        public string DatetimeModified { get; set; }
        public Guid OfficeID { get; set; }
        public string UserPermission { get; set; }
    }
}
